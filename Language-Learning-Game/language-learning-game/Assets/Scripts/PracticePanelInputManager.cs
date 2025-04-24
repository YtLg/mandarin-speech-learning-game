using Newtonsoft.Json;
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
    private TextMeshProUGUI recordButtonText;
    public Button backButton;

    private AudioClip referenceAudioClip;
    public SpeechInputManager speechInputManager;
    TranscriptButtonData transcriptButtonData1;
    public OutputPanelManager outputPanelManager;
    public PracticePanelOutputManager practicePanelOutputManager;
    public AudioManager audioManager;
    public ApiManager apiManager;


    private int currentLanguageID = 0;
    private string chinese;
    private string pinyin;
    private string english;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        // hides the panel on startup
        practicePanel.SetActive(false);

        // sets up button listeners
        replayButton.onClick.AddListener(ReplayButtonPressed);
        recordButton.onClick.AddListener(RecordButtonPressed);
        translateButton.onClick.AddListener(TranslateButtonPressed);
        backButton.onClick.AddListener(BackButtonPressed);
        recordButtonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    // sets up the display elements of the panel with the correct information that was passed in
    public void SetupPanel(TranscriptButtonData transcriptButtonData)
    {
        transcriptButtonData1 = transcriptButtonData;
        chinese = transcriptButtonData.buttonTextData;
        GetTranslations(transcriptButtonData.buttonTextData);
        practiceDisplayText.text = transcriptButtonData.buttonTextData;
        referenceAudioClip = transcriptButtonData.textAudio;
        practicePanel.SetActive (true);
    }

    // unhides the panel.
    public void ShowPanel()
    {
        practicePanel.SetActive (true);
    }

    // Sends the data to the output panel after speech input is completed.
    public void SpeechCompleted(AnalysisData analysisData, AudioClip recordedClip)
    {
        practicePanelOutputManager.SetupDisplayOutputPanel(analysisData, recordedClip, transcriptButtonData1.textAudio, transcriptButtonData1);
        recordButtonText.text = "Press to Start Recording";
        practicePanel.SetActive(false);
    }

    // -------- BUTTON HANDLERS --------


    // Handles start and stopping recording + sending audio to the API and recieving the returned output.
    async void RecordButtonPressed()
    {
        if (!speechInputManager.isRecording) // False vs ! for readability.
        {
            Debug.Log("Recording Started!");
            speechInputManager.StartRecording();
            recordButtonText.text = "Press to End Recording";

        }
        else
        {
            Debug.Log("Recording Ended!!");
            recordButtonText.text = "Processing your input...";
            (byte[] recordedBytes, AudioClip recordedClip) = speechInputManager.StopRecording();
            Debug.Log(recordedBytes);
            Debug.Log(transcriptButtonData1);
            AnalysisData analysisData = await speechInputManager.SendAnalysisAudio(transcriptButtonData1, recordedBytes);
            if (analysisData == null)
            {
                recordButtonText.text = "Press to Start Recording";

            }
            SpeechCompleted(analysisData, recordedClip);
        }
    }

    public void ReplayButtonPressed()
    {
        audioManager.PlaySegment(referenceAudioClip, transcriptButtonData1.startTime, transcriptButtonData1.endTime);
    }

    async void GetTranslations(string characters)
    {
        var result = await apiManager.TranslateCharacter(characters);
        TranslationData translationData = JsonConvert.DeserializeObject<TranslationData>(result);
        pinyin = translationData.pinyin;
        english = translationData.english;
    }

    public void TranslateButtonPressed()
    {
        if (currentLanguageID >= 2) // wraps around back to 0 if on 2 or above.
        {
            currentLanguageID = 0;
        }
        else
        {
            currentLanguageID += 1; // otherwise it just increments.
        }

        ChangeDialogueText(currentLanguageID);
    }

    public void BackButtonPressed()
    {
        practicePanel.SetActive(false);
        outputPanelManager.DisplayThePanel();
    }

    public void ChangeDialogueText(int langID)
    {
        if (langID == 0)
        {
            practiceDisplayText.text = chinese;
        }
        else if (langID == 1)
        {
            practiceDisplayText.text = pinyin;
        }
        else
        {
            practiceDisplayText.text = english;
        }

    }
}

public class TranslationData
{
    public string pinyin;
    public string english;
}