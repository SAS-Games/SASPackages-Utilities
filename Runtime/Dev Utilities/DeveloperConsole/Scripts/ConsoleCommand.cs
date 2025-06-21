using UnityEngine;

namespace SAS.Utilities.DeveloperConsole
{
    public abstract class ConsoleCommand : ScriptableObject, IConsoleCommand
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public string[] Presets { get; private set; }
        public abstract string HelpText { get; }
        public abstract bool Process(DeveloperConsoleBehaviour developerConsole, string[] args = null);
    }
}