using TMPro;
using UnityEngine;

public class PracticePanelManager : MonoBehaviour
{

    public GameObject practicePanel;
    public TextMeshProUGUI practiceDisplayText;
    public AudioClip referenceAudioClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        practicePanel.SetActive(false);
    }

    public void SetupPanel(TranscriptButtonData transcriptButtonData, AudioClip choiceAudio)
    {
        practiceDisplayText.text = transcriptButtonData.buttonTextData;
        referenceAudioClip = choiceAudio;
        practicePanel.SetActive (true);
    }

    void recordingButtonPressed()
    {
        // Make a recording button to press
        // Have reference speechInputManager and have it start recording and stop recording when pressed with start/stopRecording()
        // Modify SpeechInputManager stopRecording() to accept a bool (0/1), if 0, then it will send audio to whisper, if 1 then it will send audio to parselmouth via APImanager.
        // When it returns with the data, swap to output panel.
    }
}
