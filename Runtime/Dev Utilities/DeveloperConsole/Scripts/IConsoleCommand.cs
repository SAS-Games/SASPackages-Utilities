namespace SAS.Utilities.DeveloperConsole
{
    public interface IConsoleCommand
    {
        string CommandWord { get; }
        string HelpText { get; }
        bool Process(DeveloperConsoleBehaviour developerConsole, string[] args = null);
    }
}
