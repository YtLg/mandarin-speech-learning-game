using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;


// Displays the debug logs onto a panel, needed because there were VR headset only issues that I couldn't see the debug log of and fix.
public class DebugConsole : MonoBehaviour
{
    public TextMeshProUGUI debugText; 
    private StringBuilder logBuilder = new StringBuilder();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logBuilder.AppendLine(logString);
        if (debugText != null)
        {
            debugText.text = logBuilder.ToString();

        }
    }
}