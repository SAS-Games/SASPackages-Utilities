using UnityEngine;

namespace SAS.Utilities.DeveloperConsole
{
    public abstract class ConsoleCommand : ScriptableObject, IConsoleCommand
    {
        [SerializeField] private string commandWord = string.Empty;
        [field: SerializeField] public bool UICommand { get; private set; } = true;
        public abstract string HelpText { get; }
        public string CommandWord => commandWord;

        public abstract bool Process(DeveloperConsoleBehaviour developerConsole, string[] args = null);
    }
}