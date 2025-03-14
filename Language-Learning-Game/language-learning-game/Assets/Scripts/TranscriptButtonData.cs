using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TranscriptButtonData : MonoBehaviour
{
    OutputPanelManager outputPanelManager;
    public TextMeshProUGUI textComponent;
    private Button button;

    public AudioClip textAudio;
    public string buttonTextData;
    public float expectedProb;
    public float actualProb;
    public float startTime;
    public float endTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        outputPanelManager = FindFirstObjectByType<OutputPanelManager>();
        button.onClick.AddListener(() => outputPanelManager.TranscriptButtonPressed(gameObject));
    }

    public void SetText()
    {
        textComponent.text = buttonTextData;
        SetColour();
    }


    public void SetColour()
    {
        float percent = CalcPercentage(actualProb, expectedProb);
        Debug.Log("percent of " + buttonTextData + "is " + percent);
        if (percent >= 70)
        {
            textComponent.color = Color.green;
        }
        else if(float.IsNaN(percent))
        {
            textComponent.color = Color.red;
        }
        else
        {
            textComponent.color = Color.yellow;
        }
    }

    public float CalcPercentage(float a, float b)
    {
        float temp = a / b;
        return (temp * 100);
    }
}
