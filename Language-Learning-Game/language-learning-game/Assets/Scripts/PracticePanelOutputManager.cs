using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;
using System;
using TMPro;
using System.Linq;
using System.Reflection.Emit;

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
        feedbackText.text = "";

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
            feedbackText.text += analysisData.feedbackList[i] + "<br>";
        }

        // CHART SETUP -------------------------
        lineChart.RemoveAllSerie();

        // Legend, Axis + Title text setup because manual setup in inspector is reset per refresh:
        Legend legend = lineChart.GetChartComponent<Legend>();
        Title title = lineChart.GetChartComponent<Title>();
        XAxis xAxis = lineChart.GetChartComponent<XAxis>();
        YAxis yAxis = lineChart.GetChartComponent<YAxis>();

        legend.itemWidth = 20;
        legend.itemHeight = 15;
        legend.labelStyle.textStyle.fontSize = 30;
        legend.labelStyle.textStyle.fontStyle = FontStyle.Bold;
        legend.labelStyle.width = 20;
        legend.labelStyle.height = 20;
        legend.itemGap = 60;

        legend.positions.Add(new Vector3(-300,300,0));
        legend.positions.Add(new Vector3(100, 300, 0));

        title.labelStyle.textStyle.fontSize = 50;
        title.labelStyle.textStyle.fontStyle = FontStyle.Bold;

        yAxis.axisName.labelStyle.rotate = 90;
        yAxis.axisName.labelStyle.offset = new Vector3(-50, -250, 0);
        yAxis.axisName.labelStyle.textStyle.fontSize = 25;
        yAxis.axisName.labelStyle.textStyle.fontStyle = FontStyle.Bold;

        xAxis.axisName.labelStyle.offset = new Vector3(-415, -40, 0);
        xAxis.axisName.labelStyle.textStyle.fontSize = 25;
        xAxis.axisName.labelStyle.textStyle.fontStyle = FontStyle.Bold;


        // Populates the graph with recording pitch values.
        Serie serie1 = lineChart.AddSerie<Line>("Recording Pitch");
        serie1.symbol.show = true;
        serie1.stack = "PitchStack1";
        for (int i = 0; i < analysisData.pitchRecording.Length; i++) {
            float temp = MathF.Round(analysisData.timestampsRecording[i] - analysisData.timestampsRecording[0], 2);
            SerieData datapoint = serie1.AddXYData(temp, analysisData.pitchRecording[i]);
            if (analysisData.relevantDeviations.Contains(i))
            {
                datapoint.EnsureComponent<ItemStyle>();
                datapoint.EnsureComponent<LineStyle>();
                datapoint.itemStyle.color = Color.red;
                datapoint.lineStyle.color = Color.red;
            }
        }

        // Populates the graph with reference pitch values.
        Serie serie2 = lineChart.AddSerie<Line>("Reference Pitch");
        serie2.stack = "PitchStack2";
        for (int i = 0; i < analysisData.pitchReference.Length; i++)
        {
            float temp = MathF.Round(analysisData.timestampsReference[i] - analysisData.timestampsReference[0], 2);
            SerieData datapoint = serie2.AddXYData(temp, analysisData.pitchReference[i]);
            if (analysisData.relevantDeviations.Contains(i))
            {
                datapoint.EnsureComponent<ItemStyle>(); // There is no component by default
                datapoint.EnsureComponent<LineStyle>(); // So ensure creates one if there is none.
                datapoint.itemStyle.color = Color.red;
                datapoint.lineStyle.color = Color.red;
            }
            
            //datapoint.symbol.color = Color.red;
            //datapoint.symbol.size = 20;
            //datapoint.symbol.type = SymbolType.Diamond;
        }

        Debug.Log(analysisData.relevantDeviations);
        lineChart.RefreshChart();

        // --------------------------------------------
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
