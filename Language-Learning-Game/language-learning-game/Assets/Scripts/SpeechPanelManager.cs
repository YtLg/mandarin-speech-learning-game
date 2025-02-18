using System.Text.RegularExpressions;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class SpeechPanelManager : MonoBehaviour
{
    /// <summary>
    /// This panel will display the intial UI for speech input of the selected choice passed in.
    /// It will also communicate with the dialogue manager to notify the end of speech input section.
    /// And call methods from SpeechInputManager for speech to text functionality.
    /// </summary>
 
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
        translateButton.onClick.AddListener(translateButtonPressed);
        replayButton.onClick.AddListener(replayButtonPressed);
        recordButton.onClick.AddListener(recordButtonPressed);
       
    }

    public void displaySpeechPanel(ButtonDataScript buttonData)
    {
        dialoguePanel.SetActive(true);
        selectedChoiceData = buttonData;
        displayText.text = selectedChoiceData.englishText;
    }

    public void showSpeechPanel()
    {
        dialoguePanel.SetActive(true);
    }

    public void resetSpeechPanel()
    {
        // implementation to wipe and reset data.
    }

    public void speechCompleted(WhisperData data)
    {
        
    }


    // ---- Button Press Listeners ----

    async void recordButtonPressed()
    {
        if (!speechInputManager.isRecording) // False vs ! for readability.
        {
            speechInputManager.StartRecording();
        }
        else
        {
            WhisperData data = await speechInputManager.StopRecording();
            speechCompleted(data);
        }
    }

    void translateButtonPressed()
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

    void replayButtonPressed()
    {
        
    }


    // -- HELPER FUNCTIONS ---

    public void ChangeDisplayText(int langID)
    {
        if (langID == 0)
        {
            displayText.text = selectedChoiceData.englishText;
        }
        else if (langID == 1)
        {
            displayText.text = selectedChoiceData.pinyinText;
        }
        else
        {
            displayText.text = selectedChoiceData.chineseText;
        }

    }

    public string CleanText(string text)
    {
        // Remove punctuation, make the text lowercase, and remove spaces
        return Regex.Replace(text.ToLower(), @"[^\w]", "");
    }

}
