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
    [SerializeField] private GameObject m_OnScreenLogPrefab;
    public override string HelpText => m_HelpText;

    private GameObject _onScreenLog;

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

    public void ShowOnScreen(string[] args)
    {
        if (args.Length < 1)
            return;
        
        if (args[0].Equals("On", StringComparison.OrdinalIgnoreCase))
        {
            if (_onScreenLog == null)
                _onScreenLog = Instantiate(m_OnScreenLogPrefab);
            _onScreenLog.SetActive(true);
        }
        else if (args[0].Equals("Off", StringComparison.OrdinalIgnoreCase))
            _onScreenLog.SetActive(false);
    }
}