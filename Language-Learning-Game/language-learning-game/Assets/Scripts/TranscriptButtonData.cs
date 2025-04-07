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
    private Color orange = new Color(1f, 0.5f, 0f);
    private Color yellowGreen = new Color(0.6f, 1f, 0f);
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

        if (percent >= 70)
        {
            textComponent.color = Color.green;
        }
        else if(percent >= 59.5)
        {
            textComponent.color = yellowGreen;
        }
        else if (percent >= 40)
        {
            textComponent.color = Color.yellow;
        }
        else if (float.IsNaN(percent) || percent <= 1)
        {
            textComponent.color = Color.red;
        }
        else
        {
            textComponent.color = orange;  // orange colour!
        }

    }

    public float CalcPercentage(float a, float b)
    {
        float temp = a / b;
        return (temp * 100);
    }
}
