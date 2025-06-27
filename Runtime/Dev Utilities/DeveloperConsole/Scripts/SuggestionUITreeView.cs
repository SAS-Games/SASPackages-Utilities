using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SAS.Utilities.DeveloperConsole
{
    public class SuggestionUITreeView : MonoBehaviour
    {
        [SerializeField] private RectTransform m_BaseCommandContainer;
        [SerializeField] private GameObject m_BaseCommandTemplate;
        [SerializeField] private GameObject m_PresetTemplate;

        private List<GameObject> _activeCommandObjects = new();
        private List<GameObject> _navigableItems = new();
        private GameObject _currentlyExpanded = null;
        private GameObject _highlightedItem = null;
        private int _selectedIndex = -1;

        private DeveloperConsole _developerConsole;
        private DeveloperConsoleBehaviour _developerConsoleUI;
        private ConsoleInputActions _inputActions;

        private void Awake()
        {
            _inputActions = new ConsoleInputActions();
            _inputActions.Developer.Navigate.performed += callbackContext => Navigate(callbackContext.ReadValue<Vector2>());
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
            if (!gameObject.activeInHierarchy || _navigableItems.Count == 0)
                return;

            Vector2 nav = readValue;

            if (nav.y > 0.1f)
            {
                _selectedIndex = (_selectedIndex - 1 + _navigableItems.Count) % _navigableItems.Count;
                HighlightSelection();
            }
            else if (nav.y < -0.1f)
            {
                _selectedIndex = (_selectedIndex + 1) % _navigableItems.Count;
                HighlightSelection();
            }
        }

        private void RebuildNavigableList()
        {
            _navigableItems.Clear();
            foreach (var baseItem in _activeCommandObjects)
            {
                _navigableItems.Add(baseItem);

                RectTransform presetContainer = baseItem.transform.Find("PresetContainer") as RectTransform;
                if (presetContainer != null && presetContainer.gameObject.activeSelf)
                {
                    foreach (Transform child in presetContainer)
                    {
                        if (child.gameObject != null && child.gameObject.activeSelf)
                            _navigableItems.Add(child.gameObject);
                    }
                }
            }

            if (_selectedIndex >= _navigableItems.Count)
                _selectedIndex = _navigableItems.Count - 1;
        }

        private void HighlightSelection()
        {
            if (_highlightedItem != null)
            {
                var text = _highlightedItem.GetComponentInChildren<TMP_Text>();
                text.color = Color.white;
            }

            if (_selectedIndex >= 0 && _selectedIndex < _navigableItems.Count)
            {
                _highlightedItem = _navigableItems[_selectedIndex];
                var text = _highlightedItem.GetComponentInChildren<TMP_Text>();
                text.color = Color.yellow;
                Debug.Log($"{text.text} {_highlightedItem.GetComponentInChildren<Button>().transform.parent.name}");
                StartCoroutine(SelectGameObjectNextFrame(_highlightedItem.GetComponentInChildren<Button>().gameObject));

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

            RectTransform presetContainer = baseItem.transform.Find("PresetContainer").GetComponent<RectTransform>();
            presetContainer.gameObject.SetActive(false);

            Button toggleButton = baseItem.GetComponentInChildren<Button>();
            toggleButton.onClick.AddListener(() => { OnCommandSelected(label, baseCommand, presetContainer); });

            _activeCommandObjects.Add(baseItem);
        }

        private void OnCommandSelected(TMP_Text tmpText, string baseCommand, RectTransform presetContainer)
        {
            if (_currentlyExpanded != null && _currentlyExpanded != presetContainer.gameObject)
                _currentlyExpanded.SetActive(false);

            // Toggle current one
            bool isActive = presetContainer.gameObject.activeSelf;
            presetContainer.gameObject.SetActive(!isActive);
            _currentlyExpanded = presetContainer.gameObject.activeSelf ? presetContainer.gameObject : null;

            if (_currentlyExpanded != null)
            {
                var suggestions = _developerConsole.GetCommandSuggestions(baseCommand);
                suggestions = suggestions.Where(s => s != baseCommand)
                    .ToList();

                CreatePresetUI(tmpText.renderedWidth, presetContainer, suggestions);
            }

            RebuildNavigableList();
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_BaseCommandContainer);
        }

        private void CreatePresetUI(float startPos, RectTransform presetCommandsContainer, List<string> suggestions)
        {
            // Clear old presets
            foreach (Transform child in presetCommandsContainer)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            foreach (string suggestion in suggestions)
            {
                GameObject presetItem = Instantiate(m_PresetTemplate, presetCommandsContainer);
                presetItem.SetActive(true);

                TMP_Text label = presetItem.GetComponentInChildren<TMP_Text>();
                label.text = suggestion;
                LayoutRebuilder.ForceRebuildLayoutImmediate(presetCommandsContainer);

                Button presetButton = presetItem.GetComponent<Button>();
                //presetButton.onClick.AddListener(() => onSuggestionSelected?.Invoke(suggestion));
            }

            VerticalLayoutGroup layoutGroup = presetCommandsContainer.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup != null)
                layoutGroup.padding.left = Mathf.RoundToInt(startPos);
        }

        private void Hide()
        {
            ClearSuggestions();
        }

        private void ClearSuggestions()
        {
            foreach (var go in _activeCommandObjects)
                Destroy(go);
            _activeCommandObjects.Clear();
            _currentlyExpanded = null;
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