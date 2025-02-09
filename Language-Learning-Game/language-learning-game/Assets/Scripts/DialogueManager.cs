using TMPro;
using UnityEngine;
using System.Collections.Generic; // needed for list DON'T DELETE
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;


public class DialogueManager : MonoBehaviour
{
    public Transform playerTransform;

    // DIALOGUE CANVAS PANELS //
    public GameObject dialogueCanvas;
    public GameObject dialoguePanel;
    public GameObject choicePanel;
    public GameObject speechInputPanel;

    // DIALOGUE ELEMENTS //
    private TextMeshProUGUI speakerNameText;
    private TextMeshProUGUI dialogueText;
    public Button continueButton;
    private Button exitButton;

    // DIALOGUE DATA STORE //
    private DialogueElementsScript currentDialogue;
    private DialogueElementsScript.DialogueElementList currentDialogueLine;
    private int currentElementIndex;

    // CHOICE ELEMENTS + DATA //
    public GameObject choiceButtonPrefab;
    private List<GameObject> choiceButtonList = new List<GameObject>();

    // SPEECH INPUT ELEMENTS + DATA //
    public Button SpeechInputButton;
    private TextMeshProUGUI SpeechInputText;
    public Button SpeechInputExitButton;
    public Button SpeechInputTranslateButton;
    public Button SpeechInputRepeatButton;
    private ButtonDataScript selectedChoiceData;

    public SpeechManager dialogueManager;

    int currentLanguageID; // 0 = English | 1 = PinYin | 2 = Chinese

    // ---- initial setup --- //
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Set up dialogue text values
        speakerNameText = dialoguePanel.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        dialogueText = dialoguePanel.transform.Find("Dialogue").GetComponent<TextMeshProUGUI>();
        SpeechInputText = speechInputPanel.transform.Find("SpeechInputText").GetComponent<TextMeshProUGUI>();
    }

    public void ShowCanvas(Transform npcTransform, Vector3 offset, DialogueElementsScript dialogueElements)
    {
        currentLanguageID = 0;
        currentElementIndex = 0; // Start at 0

        speechInputPanel.SetActive(false);
        choicePanel.SetActive(false);
        dialogueCanvas.SetActive(true); // Show the canvas

        currentDialogue = dialogueElements; // Pass it to a global variable
        currentDialogueLine = currentDialogue.dialogueElementList[currentElementIndex]; // Store the current dialogue element

        // Calculate the canvas position to the left of the NPC relative to the player's perspective
        Vector3 directionToNPC = npcTransform.position - playerTransform.position;
        directionToNPC.y = 0; // Ignore vertical difference (optional, depending on your game)
        directionToNPC.Normalize();

        // Calculate the left direction relative to the player's perspective
        Vector3 leftDirection = -Vector3.Cross(directionToNPC, Vector3.up).normalized;

        // Set the canvas position
        dialogueCanvas.transform.position = npcTransform.position + leftDirection * offset.magnitude + offset;

        DisplayDialogue();
    }

    private void DisplayDialogue()
    {
        dialoguePanel.SetActive(true);
       // If current dialogue has a choice, hide the continue button
       if (currentDialogueLine.hasChoice)
        {
            continueButton.gameObject.SetActive(false); // Hide continue button
            SetupChoicePanel(); // Then generate + show the choice buttons + panel
        }
        else
        {
            continueButton.gameObject.SetActive(true);
        }

        speakerNameText.text = currentDialogue.name;
        ChangeDialogueText(currentLanguageID);
    }

    // Sets up the panel to display choice buttons when valid
    private void SetupChoicePanel()
    {
        choiceButtonList.Clear();// clear the list of buttons.

        // Clear the choice buttons from before from scene
        foreach (Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Generate the buttons
        for (int i = 0; i < currentDialogueLine.choices.Count; i++)
        {
            DialogueElementsScript.ChoiceElement choice = currentDialogueLine.choices[i];
            GameObject button = Instantiate(choiceButtonPrefab, choicePanel.transform);
            
            ButtonDataScript buttonDataScript = button.GetComponent<ButtonDataScript>();

            buttonDataScript.textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonDataScript.englishText = choice.englishChoice;
            buttonDataScript.pinyinText = choice.pinyinChoice;
            buttonDataScript.chineseText = choice.chineseChoice;
            buttonDataScript.nextNode = currentDialogueLine.nextElementID[i];
            buttonDataScript.ChangeDisplayText(currentLanguageID);

            choiceButtonList.Add(button); // add it to a list so it can change later.   
        }

        choicePanel.SetActive(true); // show it.
    }


    // ------------------------- BUTTON PRESS LISTENERS ------------------------------ //
    public void ContinueButtonPressed() // public to be accessible by button assignment in inspector
    {
        if (currentDialogue == null || currentDialogueLine.nextElementID.Count == 0) // if there is no dialogue and the continue button is pressed, then end it.
        {
            EndDialogue();
            return;
        }
        currentElementIndex = currentDialogueLine.nextElementID[0];
        currentDialogueLine = currentDialogue.dialogueElementList[currentElementIndex]; // Otherwise, move onto the next dialogue element
        DisplayDialogue(); //then display it.
    }

    public void ChoiceSelected(GameObject clickedButton) // same reasoning as before for public. // For when a choice button is clicked.
    {
        selectedChoiceData = clickedButton.GetComponent<ButtonDataScript>();
        Debug.Log(selectedChoiceData.englishText);
        string compound = "You need to say: " + selectedChoiceData.englishText;
        SpeechInputText.text = compound;
        choicePanel.SetActive(false);
        dialoguePanel.SetActive(false);
        speechInputPanel.SetActive(true);        
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

        if (currentDialogueLine.hasChoice)
        {
            // Gets the CurrentDialogueLine
            for (int i = 0; i < choiceButtonList.Count; i++)
            {
                GameObject currentButton = choiceButtonList[i];
                ButtonDataScript currentDataScript = currentButton.GetComponent<ButtonDataScript>();
                currentDataScript.ChangeDisplayText(currentLanguageID);
            }
        }
    }

    public void RecordingFinished(string transcript)
    {
        Debug.Log(CleanText(transcript));
        Debug.Log(CleanText(selectedChoiceData.englishText));
        if (CleanText(transcript) == CleanText(selectedChoiceData.englishText))
        {
            SpeechInputText.color = Color.green;
        }
        else
        {
            SpeechInputText.color= Color.red;
        }
    }

    // ------------------------ END DIALOGUE ---------------------- //

    public void EndDialogue()
    {
        // reset visibilty of everything.
        dialogueCanvas.SetActive(false);
        dialoguePanel.SetActive(false);
        speechInputPanel.SetActive(false);
        currentDialogue = null;
    }


    //--------------------------------------------------------------------------------------------------------------------------//

    // UTILITY FUNCTIONS // 

    public void ChangeDialogueText(int langID)
    {
        if (langID == 0)
        {
            dialogueText.text = currentDialogueLine.englishText;
        }
        else if(langID == 1) {
            dialogueText.text = currentDialogueLine.pinyinText;
        }
        else
        {
            dialogueText.text = currentDialogueLine.chineseText;
        }

    }

    public string CleanText(string text)
    {
        // Remove punctuation, make the text lowercase, and remove spaces
        return Regex.Replace(text.ToLower(), @"[^\w]", "");
    }

}
