using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using TMPro;

public class SuggestionUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private GameObject suggestionTemplate;

    public Action<string> onSuggestionSelected;

    private List<GameObject> activeSuggestions = new();
    private int selectedIndex = -1;

    private ConsoleInputActions inputActions;

    private void Awake()
    {
        suggestionTemplate.SetActive(false);
        ClearSuggestions();

        inputActions = new ConsoleInputActions();
        inputActions.Developer.Navigate.performed += callbackContext => Navigate(callbackContext.ReadValue<Vector2>());
        inputActions.Developer.AutoComplete.performed += _ => SelectCurrent();
    }

    private void OnEnable() => inputActions.Developer.Enable();
    private void OnDisable() => inputActions.Developer.Disable();
    private void OnDestroy() => inputActions.Dispose();

    public void ShowSuggestions(List<string> suggestions)
    {
        ClearSuggestions();

        if (suggestions == null || suggestions.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        for (int i = 0; i < suggestions.Count; i++)
        {
            string suggestion = suggestions[i];
            GameObject item = Instantiate(suggestionTemplate, container);
            item.SetActive(true);
            var text = item.GetComponentInChildren<TMP_Text>();
            text.text = suggestion;

            var button = item.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
                button.onClick.AddListener(() => onSuggestionSelected?.Invoke(suggestion));

            activeSuggestions.Add(item);
        }

        selectedIndex = 0;
        HighlightSelection();
    }

    private void Navigate(Vector2 direction)
    {
        if (activeSuggestions.Count == 0) return;
        if (direction.y > 0)
            selectedIndex = Mathf.Max(selectedIndex - 1, 0);
        else if (direction.y < 0)
            selectedIndex = Mathf.Min(selectedIndex + 1, activeSuggestions.Count - 1);
        HighlightSelection();
    }

    private void SelectCurrent()
    {
        if (selectedIndex >= 0 && selectedIndex < activeSuggestions.Count)
        {
            string selected = activeSuggestions[selectedIndex].GetComponentInChildren<TMP_Text>().text;
            onSuggestionSelected?.Invoke(selected);
        }
    }

    private void HighlightSelection()
    {
        for (int i = 0; i < activeSuggestions.Count; i++)
        {
            var text = activeSuggestions[i].GetComponentInChildren<TMP_Text>();
            text.color = (i == selectedIndex) ? Color.yellow : Color.white;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ClearSuggestions();
    }

    private void ClearSuggestions()
    {
        foreach (var go in activeSuggestions)
            Destroy(go);
        activeSuggestions.Clear();
        selectedIndex = -1;
    }
}