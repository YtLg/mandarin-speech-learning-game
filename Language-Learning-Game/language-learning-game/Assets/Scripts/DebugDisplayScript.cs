using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class DebugConsole : MonoBehaviour
{
    public TextMeshProUGUI debugText; // Assign this via the Inspector
    public ScrollRect scrollRect; // Optional: to auto-scroll the view
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

            // Optional: Auto-scroll to bottom if using a ScrollRect
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}