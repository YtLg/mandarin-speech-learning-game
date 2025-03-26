using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;
using System;
using TMPro;

public class PracticePanelOutputManager : MonoBehaviour
{

    // Display Elements -----
    public GameObject practicePanelOutput;
    AudioClip recordingAudio;
    AudioClip referenceAudio;
    AnalysisData analysisData;
    TranscriptButtonData buttonData;

    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;


    // Graph References--------
    public LineChart lineChart;
    public GameObject graphContainer;

    // Button Interactibles-----
    public Button tryAgainButton;
    public Button returnButton;
    public Button hearRecordingButton;
    public Button hearReferenceButton;

    // Object References
    public OutputPanelManager outputPanelManager;
    public PracticePanelInputManager practicePanelInputManager;
    public AudioManager audioManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        practicePanelOutput.SetActive(false);
        tryAgainButton.onClick.AddListener(TryAgainPressed);
        returnButton.onClick.AddListener(ReturnPressed);
        hearRecordingButton.onClick.AddListener(HearRecordingButtonPressed);
        hearReferenceButton.onClick.AddListener(HearReferenceButtonPressed);

    }

    public void SetupDisplayOutputPanel(AnalysisData analysisDataInput, AudioClip recordedSpeech, AudioClip referenceSpeech, TranscriptButtonData transcriptButtonData)
    {
        buttonData = transcriptButtonData;
        analysisData = analysisDataInput;
        recordingAudio = recordedSpeech;
        referenceAudio = referenceSpeech;

        Debug.Log("Pitch Recording: " + string.Join(", ", analysisData.pitchRecording));
        Debug.Log(analysisData.pitchRecording.Length);
        Debug.Log("---------------------------");
        Debug.Log("Timestamps Recording: " + string.Join(", ", analysisData.timestampsRecording));
        Debug.Log(analysisData.timestampsRecording.Length);
        Debug.Log("---------------------------");
        Debug.Log("Pitch Reference: " + string.Join(", ", analysisData.pitchReference));
        Debug.Log(analysisData.pitchReference.Length);
        Debug.Log("---------------------------");
        Debug.Log("Timestamps Reference: " + string.Join(", ", analysisData.timestampsReference));
        Debug.Log(analysisData.timestampsReference.Length);
        Debug.Log("---------------------------");

        scoreText.text = analysisData.accuracyScore + "%";
        
        for(int i = 0; i < analysisData.feedbackList.Length; i++)
        {
            Debug.Log(analysisData.feedbackList[i]);
            feedbackText.text += analysisData.feedbackList[i] + "/r";
        }

        lineChart.RemoveAllSerie();


        Serie serie1 = lineChart.AddSerie<Line>("Recording Pitch");
        serie1.stack = "PitchStack1";
        for (int i = 0; i < analysisData.pitchRecording.Length; i++) {
            float temp = MathF.Round(analysisData.timestampsRecording[i] - analysisData.timestampsRecording[0], 2);
            print(temp);
            serie1.AddXYData(temp, analysisData.pitchRecording[i]);
        }

        Serie serie2 = lineChart.AddSerie<Line>("Reference Pitch");
        serie2.stack = "PitchStack2";
        for (int i = 0; i < analysisData.pitchReference.Length; i++)
        {
            float temp = MathF.Round(analysisData.timestampsReference[i] - analysisData.timestampsReference[0], 2);
            print(temp);
            serie2.AddXYData(temp, analysisData.pitchReference[i]);

        }

        lineChart.RefreshChart();

        practicePanelOutput.SetActive(true);
    }

    // -------------------- BUTTON LISTENER FUNCTIONS ----------------
    public void HearRecordingButtonPressed()
    {
        audioManager.PlayDialogue(recordingAudio);
    }

    public void HearReferenceButtonPressed()
    {
        audioManager.PlaySegment(referenceAudio, buttonData.startTime, buttonData.endTime);
    }

    public void TryAgainPressed()
    {
        practicePanelOutput.SetActive(false);
        practicePanelInputManager.ShowPanel();
    }

    public void ReturnPressed()
    {
        practicePanelOutput.SetActive(false);
        outputPanelManager.DisplayThePanel();
    }
}
