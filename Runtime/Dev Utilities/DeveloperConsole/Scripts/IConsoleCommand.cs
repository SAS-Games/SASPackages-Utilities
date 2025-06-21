namespace SAS.Utilities.DeveloperConsole
{
    public interface IConsoleCommand
    {
        string Name { get; }
        string[] Presets { get; }
        string HelpText { get; }
        bool Process(DeveloperConsoleBehaviour developerConsole, string[] args = null);
    }
}
