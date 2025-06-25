using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SAS.Utilities.DeveloperConsole
{
    public class SuggestionUITreeView : MonoBehaviour
    {
        [SerializeField] private RectTransform container;
        [SerializeField] private GameObject baseCommandTemplate;
        [SerializeField] private GameObject presetTemplate;

        public Action<string> onSuggestionSelected;

        private List<GameObject> activeCommandObjects = new();
        private GameObject currentlyExpanded = null;
        private DeveloperConsole _developerConsole;
        private DeveloperConsoleBehaviour _developerConsoleUI;

        private void Awake()
        {
            _developerConsoleUI = GetComponentInParent<DeveloperConsoleBehaviour>();
            _developerConsole = _developerConsoleUI.DeveloperConsole;
            _developerConsoleUI.SuggestionViewChangedEvent += OnSuggestionViewChanged;
        }

        void Start()
        {
            OnSuggestionViewChanged(_developerConsoleUI.IsTreeViewSuggestion);
        }

        public void ShowCommands()
        {
            ClearSuggestions();
            var consoleCommands = _developerConsole.ConsoleCommands;
            foreach (var baseCommand in consoleCommands)
            {
                CreateBaseCommandUI(baseCommand.Name);
            }

            gameObject.SetActive(true);
        }

        private void CreateBaseCommandUI(string baseCommand)
        {
            GameObject baseItem = Instantiate(baseCommandTemplate, container);
            baseItem.SetActive(true);

            TMP_Text label = baseItem.GetComponentInChildren<TMP_Text>();
            label.text = baseCommand;

            RectTransform presetContainer = baseItem.transform.Find("PresetContainer").GetComponent<RectTransform>();
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
            }
        }

        private void CreatePresetUI(Transform parent, List<string> suggestions)
        {
            // Clear old presets
            foreach (Transform child in parent)
                Destroy(child.gameObject);

            foreach (string suggestion in suggestions)
            {
                GameObject presetItem = Instantiate(presetTemplate, parent);
                presetItem.SetActive(true);

                TMP_Text label = presetItem.GetComponentInChildren<TMP_Text>();
                label.text = suggestion;

                //Button presetButton = presetItem.GetComponent<Button>();
                //presetButton.onClick.AddListener(() => onSuggestionSelected?.Invoke(suggestion));
            }
        }

        private void Hide()
        {
            gameObject.SetActive(false);
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
    }
}