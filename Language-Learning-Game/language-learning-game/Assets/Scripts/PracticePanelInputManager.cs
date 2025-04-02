using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.Rendering;
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
    public ApiManager apiManager;


    private int currentLanguageID = 0;
    private string chinese;
    private string pinyin;
    private string english;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Start()
    {
        practicePanel.SetActive(false);
        replayButton.onClick.AddListener(ReplayButtonPressed);
        recordButton.onClick.AddListener(RecordButtonPressed);
        translateButton.onClick.AddListener(TranslateButtonPressed);
    }

    public void SetupPanel(TranscriptButtonData transcriptButtonData)
    {
        transcriptButtonData1 = transcriptButtonData;
        chinese = transcriptButtonData.buttonTextData;
        GetTranslations(transcriptButtonData.buttonTextData);
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