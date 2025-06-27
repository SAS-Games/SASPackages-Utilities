using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SAS.Utilities.DeveloperConsole
{
    public class SuggestionUITreeView : MonoBehaviour
    {
        [FormerlySerializedAs("container")] [SerializeField]
        private RectTransform m_BaseCommandContainer;

        [FormerlySerializedAs("baseCommandTemplate")] [SerializeField]
        private GameObject m_BaseCommandTemplate;

        [FormerlySerializedAs("presetTemplate")] [SerializeField]
        private GameObject m_PresetTemplate;

        public Action<string> onSuggestionSelected;

        private List<GameObject> activeCommandObjects = new();
        private GameObject currentlyExpanded = null;
        private DeveloperConsole _developerConsole;
        private DeveloperConsoleBehaviour _developerConsoleUI;
        private ConsoleInputActions _inputActions;

        private List<GameObject> navigableItems = new();

        private GameObject highlightedItem = null;
        private int selectedIndex = -1;

        private void Awake()
        {
            _inputActions = new ConsoleInputActions();
            _inputActions.Developer.Navigate.performed +=
                callbackContext => Navigate(callbackContext.ReadValue<Vector2>());
            _inputActions.Developer.AutoComplete.performed += _ => SelectCurrent();

            _developerConsoleUI = GetComponentInParent<DeveloperConsoleBehaviour>();
            _developerConsole = _developerConsoleUI.DeveloperConsole;
            _developerConsoleUI.SuggestionViewChangedEvent += OnSuggestionViewChanged;
        }

        private void OnEnable()
        {
            _inputActions.Developer.Enable();
            ShowCommands();
            gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            _inputActions.Developer.Disable();
            Hide();
        }

        private void Navigate(Vector2 readValue)
        {
            if (!gameObject.activeInHierarchy || navigableItems.Count == 0)
                return;

            Vector2 nav = readValue;

            if (nav.y > 0.1f)
            {
                selectedIndex = (selectedIndex - 1 + navigableItems.Count) % navigableItems.Count;
                HighlightSelection();
            }
            else if (nav.y < -0.1f)
            {
                selectedIndex = (selectedIndex + 1) % navigableItems.Count;
                HighlightSelection();
            }
        }

        private void RebuildNavigableList()
        {
            navigableItems.Clear();
            foreach (var baseItem in activeCommandObjects)
            {
                navigableItems.Add(baseItem);

                RectTransform presetContainer = baseItem.transform.Find("PresetContainer") as RectTransform;
                if (presetContainer != null && presetContainer.gameObject.activeSelf)
                {
                    foreach (Transform child in presetContainer)
                    {
                        navigableItems.Add(child.gameObject);
                    }
                }
            }

            if (selectedIndex >= navigableItems.Count)
                selectedIndex = navigableItems.Count - 1;
        }

        private void HighlightSelection()
        {
            if (highlightedItem != null)
            {
                var text = highlightedItem.GetComponentInChildren<TMP_Text>();
                text.color = Color.white;
            }

            if (selectedIndex >= 0 && selectedIndex < navigableItems.Count)
            {
                highlightedItem = navigableItems[selectedIndex];
                var text = highlightedItem.GetComponentInChildren<TMP_Text>();
                text.color = Color.yellow;
                Debug.Log($"{text.text} {highlightedItem.GetComponentInChildren<Button>().transform.parent.name}");
                StartCoroutine(SelectGameObjectNextFrame(highlightedItem.GetComponentInChildren<Button>().gameObject));

                // ScrollTo(highlightedItem.GetComponent<RectTransform>());
            }
        }
        
        private IEnumerator SelectGameObjectNextFrame(GameObject go)
        {
            yield return null; // Wait for one frame
            EventSystem.current.SetSelectedGameObject(null); // Optional: Clear first
            EventSystem.current.SetSelectedGameObject(go);
        }

        private void ShowCommands()
        {
            ClearSuggestions();
            var consoleCommands = _developerConsole.ConsoleCommands;
            foreach (var baseCommand in consoleCommands)
            {
                CreateBaseCommandUI(baseCommand.Name);
            }

            RebuildNavigableList();
            gameObject.SetActive(true);
        }

        private void CreateBaseCommandUI(string baseCommand)
        {
            GameObject baseItem = Instantiate(m_BaseCommandTemplate, m_BaseCommandContainer);
            baseItem.SetActive(true);

            TMP_Text label = baseItem.GetComponentInChildren<TMP_Text>();
            label.text = baseCommand;
            baseItem.name = baseCommand;

            RectTransform presetContainer =
                baseItem.transform.Find("PresetContainer").GetComponent<RectTransform>();
            presetContainer.gameObject.SetActive(false);

            Button toggleButton = baseItem.GetComponentInChildren<Button>();
            toggleButton.onClick.AddListener(() => { OnCommandSelected(baseCommand, presetContainer); });

            activeCommandObjects.Add(baseItem);
        }

        private void OnCommandSelected(string baseCommand, RectTransform presetContainer)
        {
            if (currentlyExpanded != null && currentlyExpanded != presetContainer.gameObject)
                currentlyExpanded.SetActive(false);

            // Toggle current one
            bool isActive = presetContainer.gameObject.activeSelf;
            presetContainer.gameObject.SetActive(!isActive);
            currentlyExpanded = presetContainer.gameObject.activeSelf ? presetContainer.gameObject : null;

            if (currentlyExpanded != null)
            {
                var suggestions = _developerConsole.GetCommandSuggestions(baseCommand);
                suggestions = suggestions.Where(s => s != baseCommand)
                    .ToList();

                CreatePresetUI(presetContainer, suggestions);
                RebuildNavigableList();
            }
        }

        private void CreatePresetUI(RectTransform presetCommandsContainer, List<string> suggestions)
        {
            // Clear old presets
            foreach (Transform child in presetCommandsContainer)
                Destroy(child.gameObject);

            foreach (string suggestion in suggestions)
            {
                GameObject presetItem = Instantiate(m_PresetTemplate, presetCommandsContainer);
                presetItem.SetActive(true);

                TMP_Text label = presetItem.GetComponentInChildren<TMP_Text>();
                label.text = suggestion;
                LayoutRebuilder.ForceRebuildLayoutImmediate(presetCommandsContainer);

                //Button presetButton = presetItem.GetComponent<Button>();
                //presetButton.onClick.AddListener(() => onSuggestionSelected?.Invoke(suggestion));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_BaseCommandContainer);
        }

        private void Hide()
        {
            ClearSuggestions();
        }

        private void ClearSuggestions()
        {
            foreach (var go in activeCommandObjects)
                Destroy(go);
            activeCommandObjects.Clear();
            currentlyExpanded = null;
        }

        private void OnSuggestionViewChanged(bool treeView)
        {
            if (!treeView)
                Hide();
            else
                ShowCommands();
            gameObject.SetActive(treeView);
        }

        private void SelectCurrent()
        {
            // if (_selectedIndex >= 0 && _selectedIndex < _activeSuggestions.Count)
            // {
            //     string selected = _activeSuggestions[_selectedIndex].GetComponentInChildren<TMP_Text>().text;
            //     _developerConsoleUI.ApplySuggestion(selected);
            // }
        }
    }
}