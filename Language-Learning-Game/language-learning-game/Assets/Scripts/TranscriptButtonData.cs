using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TranscriptButtonData : MonoBehaviour
{
    OutputPanelManager outputPanelManager;
    private Button button;

    public float buttonTextData;
    public float expectedProb;
    public float speechProb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        outputPanelManager = FindFirstObjectByType<OutputPanelManager>();
        button.onClick.AddListener(() => outputPanelManager.transcriptButtonPressed(gameObject));
    }
}
