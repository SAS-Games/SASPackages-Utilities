using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SAS.Utilities.DeveloperConsole
{
    public class ConsoleCommandMenu : MonoBehaviour
    {
        [SerializeField] private ConsoleCommandButton m_ConsoleCommandButton;
        [SerializeField] private Transform m_Content;
        private DeveloperConsoleBehaviour _developerConsoleBehaviour;
        private DeveloperConsole _developerConsole;

        private Queue<ConsoleCommandButton> _consoleCommandsButton = new Queue<ConsoleCommandButton>();
        private GameObject _eventSystemLastSelectedObject;

        private void Awake()
        {
            _developerConsoleBehaviour = GetComponentInParent<DeveloperConsoleBehaviour>();
            _developerConsole = _developerConsoleBehaviour.DeveloperConsole;
        }

        private void OnEnable()
        {
            foreach (var command in _developerConsole.Commands)
            {
                if (!_consoleCommandsButton.TryDequeue(out var commandButton))
                    commandButton = Instantiate(m_ConsoleCommandButton);
                commandButton.transform.SetParent(m_Content, false);
                commandButton.transform.SetAsLastSibling();
                commandButton.transform.name = command.CommandWord;
                commandButton.gameObject.SetActive(true);
                commandButton.Init(command, _developerConsoleBehaviour);
            }

            _eventSystemLastSelectedObject = EventSystem.current.firstSelectedGameObject;
            EventSystem.current.firstSelectedGameObject = m_Content.GetChild(0).gameObject;
        }

        void OnDisable()
        {
            foreach (Transform transform in m_Content.transform)
            {
                _consoleCommandsButton.Enqueue(transform.GetComponent<ConsoleCommandButton>());
                transform.gameObject.SetActive(false);
            }

            EventSystem.current.firstSelectedGameObject = _eventSystemLastSelectedObject;
        }
    }
}
