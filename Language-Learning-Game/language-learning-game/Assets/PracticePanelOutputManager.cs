using UnityEngine;
using UnityEngine.UI;

public class PracticePanelOutputManager : MonoBehaviour
{
    public GameObject practicePanelOutput;
    AudioClip recordingAudio;
    AudioClip referenceAudio;

    public Button tryAgainButton;
    public Button returnButton;
    public Button hearRecordingButton;
    public Button hearReferenceButton;

    public OutputPanelManager outputPanelManager;
    public PracticePanelInputManager practicePanelInputManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        practicePanelOutput.SetActive(false);
        tryAgainButton.onClick.AddListener(tryAgainPressed);
        returnButton.onClick.AddListener(returnPressed);
        hearRecordingButton.onClick.AddListener(hearRecordingButtonPressed);
        hearReferenceButton.onClick.AddListener(hearReferenceButtonPressed);

    }

    public void SetupDisplayOutputPanel(AnalysisData data, AudioClip recordedSpeech, AudioClip referenceSpeech)
    {
        recordingAudio = recordedSpeech;
        referenceAudio = referenceSpeech;
        Debug.Log(data);
    }


    // -------------------- BUTTON LISTENER FUNCTIONS ----------------
    public void hearRecordingButtonPressed()
    {

    }

    public void hearReferenceButtonPressed()
    {

    }

    public void tryAgainPressed()
    {
        practicePanelOutput.SetActive(false);
        practicePanelInputManager.ShowPanel();
    }

    public void returnPressed()
    {

    }
}
