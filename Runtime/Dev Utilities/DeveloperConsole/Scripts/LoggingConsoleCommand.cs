using System.Linq;
using SAS;
using SAS.Utilities.DeveloperConsole;
using UnityEngine;
using Debug = SAS.Debug;

[CreateAssetMenu(menuName = "SAS/Utilities/DeveloperConsole/Commands/Logging Console Command")]
public class LoggingConsoleCommand : CompositeConsoleCommand
{
    [SerializeField] private string m_HelpText;
    public override string HelpText => m_HelpText;

    public void LogLevel(string[] args)
    {
        if (args.Length == 0)
            return;
        
        int logLevel = 1;
        if (args.Contains(SAS.LogLevel.Info.ToString()))
            logLevel = logLevel << 0;
        if (args.Contains(SAS.LogLevel.Warning.ToString()))
            logLevel = logLevel << 1;
        if (args.Contains(SAS.LogLevel.Error.ToString()))
            logLevel = logLevel << 2;
        
        Debug.SetLogLevel((LogLevel)logLevel);
    }
}