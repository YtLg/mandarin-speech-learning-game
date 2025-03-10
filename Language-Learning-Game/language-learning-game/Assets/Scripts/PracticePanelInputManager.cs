using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PracticePanelInputManager : MonoBehaviour
{

    public GameObject practicePanel;
    public TextMeshProUGUI practiceDisplayText;
    public Button translateButton;
    public Button replayButton;
    public Button recordButton;

    private AudioClip referenceAudioClip;
    public SpeechInputManager speechInputManager;
    TranscriptButtonData transcriptButtonData;
    public PracticePanelOutputManager practicePanelOutputManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        practicePanel.SetActive(false);
        recordButton.onClick.AddListener(RecordButtonPressed);
    }

    public void SetupPanel(TranscriptButtonData transcriptButtonData)
    {
        practiceDisplayText.text = transcriptButtonData.buttonTextData;
        referenceAudioClip = transcriptButtonData.textAudio;
        practicePanel.SetActive (true);
    }

    public void ShowPanel()
    {
        practicePanel.SetActive (true);
    }

    public void SpeechCompleted(AnalysisData analysisData, AudioClip recordedClip)
    {
        practicePanelOutputManager.SetupDisplayOutputPanel(analysisData, recordedClip, transcriptButtonData.textAudio);
        practicePanel.SetActive(false);
    }


    async void RecordButtonPressed()
    {
        if (!speechInputManager.isRecording) // False vs ! for readability.
        {
            speechInputManager.StartRecording();

        }
        else
        {
            (byte[] recordedBytes, AudioClip recordedClip) = speechInputManager.StopRecording();
            AnalysisData analysisData = await speechInputManager.SendAnalysisAudio(transcriptButtonData, recordedBytes);
            SpeechCompleted(analysisData, recordedClip);
        }
    }
}
