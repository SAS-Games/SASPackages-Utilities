using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace SAS.Utilities.DeveloperConsole
{
    public class DeveloperConsoleBehaviour : MonoBehaviour
    {
        [SerializeField] private string m_Prefix = string.Empty;
        [SerializeField] private ConsoleCommand[] m_Commands = new ConsoleCommand[0];

        [Header("UI")] [SerializeField] private GameObject m_UiCanvas = null;
        [SerializeField] private TMP_InputField m_InputField = null;
        [SerializeField] private TMP_Text m_HelpText = null;
        [SerializeField] private bool m_PauseOnOpen = false;
        [SerializeField] private SuggestionUI m_SuggestionUI;

        private float pausedTimeScale;
        private DeveloperConsole developerConsole;
        private ConsoleInputActions inputActions;

        internal DeveloperConsole DeveloperConsole
        {
            get
            {
                if (developerConsole != null)
                {
                    return developerConsole;
                }

                return developerConsole = new DeveloperConsole(m_Prefix, m_Commands);
            }
        }

        private void Awake()
        {
            pausedTimeScale = Time.timeScale;
            inputActions = new ConsoleInputActions();
            inputActions.Developer.ToggleConsole.performed += Toggle;

            if (m_InputField != null)
                m_InputField.onValueChanged.AddListener(OnInputChanged);

            m_SuggestionUI.onSuggestionSelected = ApplySuggestion;
        }

        private void OnEnable() => inputActions.Developer.Enable();
        private void OnDisable() => inputActions.Developer.Disable();

        private void Toggle(CallbackContext context)
        {
            if (m_UiCanvas.activeSelf)
            {
                if (m_InputField != null)
                    Time.timeScale = pausedTimeScale;
                m_UiCanvas.SetActive(false);
            }
            else
            {
                if (m_PauseOnOpen)
                {
                    pausedTimeScale = Time.timeScale;
                    Time.timeScale = 0;
                }

                m_UiCanvas.SetActive(true);
                StartCoroutine(FocusInputFieldNextFrame());
            }
        }
        private IEnumerator FocusInputFieldNextFrame()
        {
            yield return null; // wait one frame
            m_InputField.ActivateInputField();
            m_InputField.Select();
        }
        public void ProcessCommand(string inputValue)
        {
            DeveloperConsole.ProcessCommand(inputValue, this);
            m_InputField.text = string.Empty;
            m_SuggestionUI.Hide();
        }

        public void DisplayHelpText(string helpText)
        {
            if (m_HelpText != null)
                m_HelpText.text = helpText;
        }


        private void OnInputChanged(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                m_SuggestionUI.Hide();
                return;
            }

            var suggestions = DeveloperConsole.GetCommandSuggestions(input);
            m_SuggestionUI.ShowSuggestions(suggestions);
        }

        private void ApplySuggestion(string suggestion)
        {
            m_InputField.text = developerConsole.Prefix + suggestion + " ";
            m_InputField.caretPosition = m_InputField.text.Length;
            m_InputField.Select();
            m_SuggestionUI.Hide();
        }
    }
}