using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechPanelManager : MonoBehaviour
{
    /// <summary>
    /// This panel will display the intial UI for speech input of the selected choice passed in.
    /// It will also communicate with the dialogue manager to notify the end of speech input section.
    /// And call methods from SpeechInputManager for speech to text functionality.
    /// </summary>

    //--- Audio Player Reference ---//
    public AudioManager audioManager;

    // ------ DATA ELEMENTS PASSED IN --------
    private ButtonDataScript selectedChoiceData;

    // ------- UI ELEMENTS Interactible ------
    public GameObject dialoguePanel;
    public Button translateButton;
    public Button replayButton;
    public  Button recordButton;


    // ------ UI ELEMENTS Display -------
    public TextMeshProUGUI displayText;


    // ------ REFERENCE TO OTHER COMPONENTS -----
    public SpeechInputManager speechInputManager;
    public OutputPanelManager outputPanelManager;

    int currentLanguageID;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hides upon startup
        dialoguePanel.SetActive(false);

        // Setup button listeners
        translateButton.onClick.AddListener(TranslateButtonPressed);
        replayButton.onClick.AddListener(ReplayButtonPressed);
        recordButton.onClick.AddListener(RecordButtonPressed);
        replayButton.onClick.AddListener(ReplayButtonPressed);

    }

    public void DisplaySpeechPanel(ButtonDataScript buttonData)
    {
        dialoguePanel.SetActive(true);
        selectedChoiceData = buttonData;
        displayText.text = selectedChoiceData.englishText;
        audioManager.PlayDialogue(selectedChoiceData.choiceAudio);//PLAYS THE DIALOGUE OF AUDIO ATTACHED TO CHOICE BUTTON DATA
    }

    public void ShowSpeechPanel()
    {
        dialoguePanel.SetActive(true);
    }

    public void ResetSpeechPanel()
    {
        // implementation to wipe and reset data.
    }

    public void SpeechCompleted(WhisperData transcriptData, AudioClip recordedSpeech, WhisperData referenceData)
    {
        outputPanelManager.SetupDisplayOutputPanel(transcriptData, recordedSpeech, referenceData, selectedChoiceData.choiceAudio);
        dialoguePanel.SetActive(false);
    }


    // ---- Button Press Listeners ----

    async void RecordButtonPressed()
    {
        if (!speechInputManager.isRecording) // False vs ! for readability.
        {
            speechInputManager.StartRecording();
            
        }
        else
        {
            (WhisperData transcriptData, AudioClip clip) = await speechInputManager.StopRecording();
            WhisperData referenceData = await speechInputManager.SendReferenceAudio(selectedChoiceData.choiceAudio);
            SpeechCompleted(transcriptData, clip, referenceData);
        }
    }

    void TranslateButtonPressed()
    {
        if (currentLanguageID >= 2) // wraps around back to 0 if on 2 or above.
        {
            currentLanguageID = 0;
        }
        else
        {
            currentLanguageID += 1; // otherwise it just increments.
        }

        ChangeDisplayText(currentLanguageID);
    }

    void ReplayButtonPressed()
    {
        audioManager.PlayDialogue(selectedChoiceData.choiceAudio);
    }


    // -- HELPER FUNCTIONS ---

    public void ChangeDisplayText(int langID)
    {
        if (langID == 0)
        {
            displayText.text = selectedChoiceData.chineseText;
        }
        else if (langID == 1)
        {
            displayText.text = selectedChoiceData.pinyinText;
        }
        else
        {
            displayText.text = selectedChoiceData.englishText;
        }

    }

    public string CleanText(string text)
    {
        // Remove punctuation, make the text lowercase, and remove spaces
        return Regex.Replace(text.ToLower(), @"[^\w]", "");
    }

}
