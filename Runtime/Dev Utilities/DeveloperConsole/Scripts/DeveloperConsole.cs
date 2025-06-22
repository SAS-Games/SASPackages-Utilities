using System;
using System.Collections.Generic;
using System.Linq;

namespace SAS.Utilities.DeveloperConsole
{
    public class DeveloperConsole
    {
        public readonly string Prefix;
        private readonly IEnumerable<IConsoleCommand> ConsoleCommands;
        private readonly CommandTrie _commandTrie = new CommandTrie();

        public DeveloperConsole(string prefix, IEnumerable<IConsoleCommand> consoleCommands)
        {
            this.Prefix = prefix;
            this.ConsoleCommands = consoleCommands;
            foreach (var consoleCommand in this.ConsoleCommands)
            {
                _commandTrie.Insert($"{this.Prefix}{consoleCommand.Name}");

                foreach (var preset in consoleCommand.Presets)
                {
                    _commandTrie.Insert($"{this.Prefix}{preset}");
                }
            }
        }

        public void ProcessCommand(string inputValue, DeveloperConsoleBehaviour developerConsole)
        {
            if (!inputValue.StartsWith(Prefix))
            {
                return;
            }

            inputValue = inputValue.Remove(0, Prefix.Length);
            string[] inputSplit = inputValue.Split(' ');

            string commandInput = inputSplit[0];
            string[] args = inputSplit.Skip(1).ToArray();
            if (inputValue.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                developerConsole.DisplayHelpText("");
                return;
            }

            ProcessCommand(commandInput, args, developerConsole);
        }

        private void ProcessCommand(string commandInput, string[] args, DeveloperConsoleBehaviour developerConsole)
        {
            foreach (var command in ConsoleCommands)
            {
                if (!command.Contains(commandInput))
                    continue;

                if (command.HelpRequest(commandInput, args, out var message))
                    developerConsole.DisplayHelpText(message);
                else
                {
                    if (!command.Process(developerConsole, commandInput, args))
                        Debug.LogError($"Failed to execute the Command '{commandInput}'");
                }

                return;
            }

            Debug.LogError($"No command found for '{commandInput}'");
        }

        public List<string> GetCommandSuggestions(string input)
        {
            return _commandTrie.GetAllWithPrefix(input);
        }
    }
}