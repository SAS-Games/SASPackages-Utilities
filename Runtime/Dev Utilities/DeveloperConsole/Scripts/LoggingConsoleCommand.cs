using System;
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
        if (args.Length < 2)
            return;
        
        LogLevel logLevel = SAS.LogLevel.None;
        if (args.Contains(SAS.LogLevel.Info.ToString()))
            logLevel = SAS.LogLevel.Info;
        if (args.Contains(SAS.LogLevel.Warning.ToString()))
            logLevel = SAS.LogLevel.Warning;
        if (args.Contains(SAS.LogLevel.Error.ToString()))
            logLevel = SAS.LogLevel.Error;

        Debug.SetLogLevel(logLevel, args[1].Equals("On", StringComparison.OrdinalIgnoreCase) ? true : false);
    }
}