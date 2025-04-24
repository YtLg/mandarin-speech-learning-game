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
    public Button backButton;


    // ------ UI ELEMENTS Display -------
    public TextMeshProUGUI displayText;
    private TextMeshProUGUI recordButtonText;

    // ------ REFERENCE TO OTHER COMPONENTS -----
    public SpeechInputManager speechInputManager;
    public OutputPanelManager outputPanelManager;
    public DialogueManager dialogueManager;
    public ObjectPanelManager objectPanelManager;

    int currentLanguageID;

    int objectOrNPC; // 0 = NPC, 1 = Object

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hides upon startup
        dialoguePanel.SetActive(false);

        // Setup button listeners
        translateButton.onClick.AddListener(TranslateButtonPressed);
        replayButton.onClick.AddListener(ReplayButtonPressed);
        recordButton.onClick.AddListener(RecordButtonPressed);
        backButton.onClick.AddListener(BackButtonPressed);
        recordButtonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    // Called to set up all the relevant display content on the panel.
    public void DisplaySpeechPanel(ButtonDataScript buttonData, int type)
    {
        currentLanguageID = 0;
        dialoguePanel.SetActive(true);
        selectedChoiceData = buttonData;
        displayText.text = selectedChoiceData.englishText;
        audioManager.PlayDialogue(selectedChoiceData.choiceAudio);//PLAYS THE DIALOGUE OF AUDIO ATTACHED TO CHOICE BUTTON DATA

        objectOrNPC = type;
    }

    // called from other panels to unhide this panel.
    public void ShowSpeechPanel()
    {
        dialoguePanel.SetActive(true);
    }

    // Sends speech data to the output panel and hides this panel.
    public void SpeechCompleted(WhisperData transcriptData, AudioClip recordedSpeech, WhisperData referenceData)
    {
        Debug.Log("Speech Completed!");
        outputPanelManager.SetupDisplayOutputPanel(transcriptData, recordedSpeech, referenceData, selectedChoiceData.choiceAudio, objectOrNPC);
        recordButtonText.text = "Press to Start Recording";
        dialoguePanel.SetActive(false);
    }


    // ---- Button Press Listeners ----

    // Handles start and end recording presses, sends speech input + reference to API.
    async void RecordButtonPressed()
    {
        if (!speechInputManager.isRecording)
        {
            speechInputManager.StartRecording();
            recordButtonText.text = "Press to Stop Recording";
            
        }
        else
        {
            recordButtonText.text = "Processing your input...";
            (byte[] recordedBytes, AudioClip recordedClip) = speechInputManager.StopRecording();
            Debug.Log("Stopped recording and data saved!");
            WhisperData transcriptData = await speechInputManager.SendRecordedAudio(recordedBytes);
            Debug.Log("Transcript data for recording done!");
            WhisperData referenceData = await speechInputManager.SendReferenceAudio(selectedChoiceData.choiceAudio);
            Debug.Log("Transcript data for reference done!");

            if (transcriptData == null || referenceData == null)
            {
                recordButtonText.text = "Press to Start Recording";

            }

            SpeechCompleted(transcriptData, recordedClip, referenceData);
        }
    }

    // translates the text to the language ID and increments ID by 1  to keep it cycling.
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

    // replays the refence audio
    void ReplayButtonPressed()
    {
        audioManager.PlayDialogue(selectedChoiceData.choiceAudio);
    }

    // goes back to the panel that led to this panel. Object panel or dialogue panel.
    private void BackButtonPressed()
    {
        dialoguePanel.SetActive(false);
        if (objectOrNPC == 0)
        {
            dialogueManager.DisplayThePanel();
        }
        else
        {
            objectPanelManager.ShowObjectPanel();
        }
    }

    // -- HELPER FUNCTIONS ---

    // changes display text based on ID passed into it.
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

    // removes, punctuation and spaces and lowercases the argument.
    public string CleanText(string text)
    {
        return Regex.Replace(text.ToLower(), @"[^\w]", "");
    }

}
