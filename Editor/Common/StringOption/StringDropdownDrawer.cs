using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(StringDropdownAttribute))]
public class StringDropdownDrawer : PropertyDrawer
{
    private const string DEFAULT_EDITOR_RESOURCE_PATH = "StringOptions/DefaultStringOptionsSO.asset";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (StringDropdownAttribute)attribute;
        var targetObject = property.serializedObject.targetObject;
        StringOptions stringOptions = null;

        // If a field name is specified, try to find it
        if (!string.IsNullOrEmpty(attr.SourceFieldName))
        {
            var fieldInfo = targetObject.GetType().GetField(attr.SourceFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
                stringOptions = fieldInfo.GetValue(targetObject) as StringOptions;
        }

        // If no field was provided or the value is null, load default
        if (stringOptions == null)
            stringOptions = EditorGUIUtility.Load(DEFAULT_EDITOR_RESOURCE_PATH) as StringOptions;

        if (stringOptions == null)
        {
            EditorGUI.HelpBox(position, $"No StringOptionsSO found. Set field or add 'Resources/{DEFAULT_EDITOR_RESOURCE_PATH}.asset'", MessageType.Error);
            return;
        }

        var availableOptions = stringOptions.Values;
        string currentValue = property.stringValue;

        bool valueInList = availableOptions.Contains(currentValue);

        List<string> displayList = new List<string>();

        if (!valueInList && !string.IsNullOrEmpty(currentValue))
            displayList.Add($"❌ {currentValue} (missing)"); // Show invalid entry with red highlight

        displayList.AddRange(availableOptions);

        int selectedIndex = Mathf.Max(0, displayList.IndexOf(currentValue));

        if (!valueInList && !string.IsNullOrEmpty(currentValue))
            selectedIndex = 0;
        else
        {
            selectedIndex = displayList.IndexOf(currentValue);
            if (selectedIndex < 0)
                selectedIndex = valueInList ? availableOptions.IndexOf(currentValue) + (valueInList ? (displayList.Count - availableOptions.Count) : 0) : 0;
        }

        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, displayList.ToArray());

        if (newIndex != selectedIndex)
        {
            if (displayList[newIndex] == "────────")
                return;

            if (!valueInList && newIndex == 0)
                return;

            property.stringValue = displayList[newIndex].Replace("❌ ", "").Replace(" (missing)", "");
        }
    }
}