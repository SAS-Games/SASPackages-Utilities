using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SAS.Utilities.DeveloperConsole
{
    public abstract class CompositeConsoleCommand : ConsoleCommand
    {
        [Serializable]
        private class SubCommand
        {
            public string Name;
            public string HelpText;
            public string[] Presets;
            public UnityEvent<string[]> Action;
        }

        [SerializeField] private List<SubCommand> subCommands = new();
        public override bool HelpRequest(string command, string[] args, out string message)
        {
            string subCommand = command.Split(".")[1];

            var sub = subCommands.Find(s => s.Name.Equals(subCommand, StringComparison.OrdinalIgnoreCase));
            if (sub == null)
            {
                message = $"Subcommand: '{subCommand} under Command: {command}' not found.";
                return true;
            }
            else
            {
                message = sub.HelpText;
                return args.Length > 0 && args[0].Equals("help", StringComparison.OrdinalIgnoreCase);
            }
        }

        public sealed override bool Process(DeveloperConsoleBehaviour developerConsole, string command, string[] args = null)
        {
            string subCommand = command.Split(".")[1];

            var sub = subCommands.Find(s => s.Name.Equals(subCommand, StringComparison.OrdinalIgnoreCase));
            if (sub == null)
                return false;

            sub.Action.Invoke(args);
            return true;
        }

        public sealed override string[] Presets
        {
            get
            {
                List<string> presets = new();
                foreach (var s in subCommands)
                {
                    presets.Add($"{Name}.{s.Name}");
                    if (s.Presets != null)
                    {
                        foreach (var preset in s.Presets)
                        {
                            presets.Add($"{Name}.{preset}");
                        }
                    }
                }

                return presets.ToArray();
            }
        }

        public override bool Contains(string commandName)
        {
            var commandSplit = commandName.Trim().Split(".");
            if (commandSplit.Length > 1)
                return base.Contains(commandSplit[0]);
            return false;
        }
    }
}