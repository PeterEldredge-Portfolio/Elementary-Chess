

using UnityEngine;
using System.Collections;

public class ConsolePrint : MonoBehaviour
{
    private string _log;
    private Queue _logQueue = new Queue();

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string newString = "\n [" + type + "] : " + logString;

        _logQueue.Enqueue(newString);

        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            _logQueue.Enqueue(newString);
        }

        _log = string.Empty;

        foreach (string log in _logQueue)
        {
            _log += log;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label(_log);
    }
}

