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
    TranscriptButtonData transcriptButtonData1;
    public PracticePanelOutputManager practicePanelOutputManager;
    public AudioManager audioManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        practicePanel.SetActive(false);
        replayButton.onClick.AddListener(ReplayButtonPressed);
        recordButton.onClick.AddListener(RecordButtonPressed);
    }

    public void SetupPanel(TranscriptButtonData transcriptButtonData)
    {
        Debug.Log("DATA IS:", transcriptButtonData);
        transcriptButtonData1 = transcriptButtonData;
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
        practicePanelOutputManager.SetupDisplayOutputPanel(analysisData, recordedClip, transcriptButtonData1.textAudio, transcriptButtonData1);
        practicePanel.SetActive(false);
    }

    // -------- BUTTON HANDLERS --------

    async void RecordButtonPressed()
    {
        if (!speechInputManager.isRecording) // False vs ! for readability.
        {
            Debug.Log("Recording Started!");
            speechInputManager.StartRecording();

        }
        else
        {
            Debug.Log("Recording Ended!!");
            (byte[] recordedBytes, AudioClip recordedClip) = speechInputManager.StopRecording();
            Debug.Log(recordedBytes);
            Debug.Log(transcriptButtonData1);
            AnalysisData analysisData = await speechInputManager.SendAnalysisAudio(transcriptButtonData1, recordedBytes);
            SpeechCompleted(analysisData, recordedClip);
        }
    }

    public void ReplayButtonPressed()
    {
        audioManager.PlaySegment(referenceAudioClip, transcriptButtonData1.startTime, transcriptButtonData1.endTime);
    }
}
