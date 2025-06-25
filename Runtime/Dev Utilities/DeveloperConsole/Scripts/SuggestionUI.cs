using System;
using System.Collections.Generic;
using SAS.Utilities.DeveloperConsole;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SuggestionUI : MonoBehaviour
{
    [FormerlySerializedAs("container")] [SerializeField]
    private RectTransform m_Container;

    [FormerlySerializedAs("suggestionTemplate")] [SerializeField]
    private GameObject m_SuggestionTemplate;

    private List<GameObject> _activeSuggestions = new();
    private int _selectedIndex = -1;

    private ConsoleInputActions _inputActions;
    private DeveloperConsoleBehaviour _developerConsoleUI;

    private void Awake()
    {
        m_SuggestionTemplate.SetActive(false);
        ClearSuggestions();

        _inputActions = new ConsoleInputActions();
        _inputActions.Developer.Navigate.performed += callbackContext => Navigate(callbackContext.ReadValue<Vector2>());
        _inputActions.Developer.AutoComplete.performed += _ => SelectCurrent();
        _developerConsoleUI = GetComponentInParent<DeveloperConsoleBehaviour>();
        _developerConsoleUI.SuggestionViewChangedEvent += OnSuggestionViewChanged;
    }

    private void Start()
    {
        OnSuggestionViewChanged(_developerConsoleUI.IsTreeViewSuggestion);
    }

    private void OnEnable() => _inputActions.Developer.Enable();
    private void OnDisable() => _inputActions.Developer.Disable();
    private void OnDestroy() => _inputActions.Dispose();

    private void ShowSuggestions(List<string> suggestions)
    {
        ClearSuggestions();

        if (suggestions == null || suggestions.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        foreach (var suggestion in suggestions)
        {
            GameObject item = Instantiate(m_SuggestionTemplate, m_Container);
            item.SetActive(true);
            var text = item.GetComponentInChildren<TMP_Text>();
            text.text = suggestion;

            var button = item.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => _developerConsoleUI.ApplySuggestion(suggestion));
            _activeSuggestions.Add(item);
        }

        _selectedIndex = 0;
        HighlightSelection();
    }

    private void Navigate(Vector2 direction)
    {
        if (_activeSuggestions.Count == 0) return;
        if (direction.y > 0)
            _selectedIndex = Mathf.Max(_selectedIndex - 1, 0);
        else if (direction.y < 0)
            _selectedIndex = Mathf.Min(_selectedIndex + 1, _activeSuggestions.Count - 1);
        HighlightSelection();
    }

    private void SelectCurrent()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _activeSuggestions.Count)
        {
            string selected = _activeSuggestions[_selectedIndex].GetComponentInChildren<TMP_Text>().text;
            _developerConsoleUI.ApplySuggestion(selected);
        }
    }

    private void HighlightSelection()
    {
        for (int i = 0; i < _activeSuggestions.Count; i++)
        {
            var text = _activeSuggestions[i].GetComponentInChildren<TMP_Text>();
            text.color = (i == _selectedIndex) ? Color.yellow : Color.white;
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        ClearSuggestions();
    }

    private void ClearSuggestions()
    {
        foreach (var go in _activeSuggestions)
            Destroy(go);
        _activeSuggestions.Clear();
        _selectedIndex = -1;
    }

    private void OnInputChanged(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            Hide();
            return;
        }

        var suggestions = _developerConsoleUI.DeveloperConsole
            .GetCommandSuggestions(input);
        ShowSuggestions(suggestions);
    }

    private void OnSuggestionViewChanged(bool treeView)
    {
        if (!treeView)
        {
            _developerConsoleUI.InputChangedEvent += OnInputChanged;
            _developerConsoleUI.SuggestionAppliedEvent += Hide;
        }
        else
        {
            _developerConsoleUI.InputChangedEvent -= OnInputChanged;
            _developerConsoleUI.SuggestionAppliedEvent -= Hide;
            Hide();
        }

        gameObject.SetActive(!treeView);
    }
}