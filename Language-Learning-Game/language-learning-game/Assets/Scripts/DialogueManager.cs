using TMPro;
using UnityEngine;
using System.Collections.Generic; // needed for list DON'T DELETE
using UnityEngine.UI;

/// <summary>
/// This script will manage the Dialogue + dialogue logic
/// As well as the showing and hiding of choice panels.
/// And communicating with the speechPanel when a choice is selected.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public Transform playerTransform;

    //--- REFERENCE TO AUDIO MANAGER ---//
    public AudioManager audioManager;

    //--- DIALOGUE CANVAS PANELS ---//
    public GameObject canvas;
    public GameObject dialoguePanel;
    public GameObject choicePanel;

    //--- DIALOGUE ELEMENTS ---//
    private TextMeshProUGUI speakerNameText;
    private TextMeshProUGUI dialogueText;

    //--- INTERACTABLE BUTTONS ---//
    public Button continueButton;
    private Button exitButton;
    public Button replayButton;

    //--- DIALOGUE DATA STORE ---//
    private DialogueElementsScript currentDialogue;
    private DialogueElementsScript.DialogueElementList currentDialogueLine;
    private int currentElementIndex;

    //--- CHOICE ELEMENTS + DATA ---//
    public GameObject choiceButtonPrefab;
    private List<GameObject> choiceButtonList = new List<GameObject>();
    private ButtonDataScript selectedChoiceData; // Data of the button pressed is stored here.
    public SpeechPanelManager speechPanelManager;

    int currentLanguageID; // 0 = English | 1 = PinYin | 2 = Chinese

    // ---- INITIAL SETUP & DISPLAY FUNCTIONS--- //
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        // Set up dialogue text values
        speakerNameText = dialoguePanel.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        dialogueText = dialoguePanel.transform.Find("Dialogue").GetComponent<TextMeshProUGUI>();


        // Set up Button Listeners
        replayButton.onClick.AddListener(ReplayButtonPressed);
        continueButton.onClick.AddListener(ContinueButtonPressed);

    }

    // This will be called to position the canvas at the correct location relative to the npc and the player, then call DisplayDialogue to set up and display the dialogue box.
    public void ShowCanvas(Transform npcTransform, Vector3 offset, DialogueElementsScript dialogueElements)
    {
        currentLanguageID = 0;
        currentElementIndex = 0; // Start at 0

        choicePanel.SetActive(false);
        canvas.SetActive(true); // Show the canvas

        currentDialogue = dialogueElements; // Pass it to a global variable
        currentDialogueLine = currentDialogue.dialogueElementList[currentElementIndex]; // Store the current dialogue element

        //Vector3 halfWayVector = (npcTransform.position + playerTransform.position).normalized;
        //float temp = halfWayVector.y;
        //halfWayVector.y = halfWayVector.x;
        //halfWayVector.x = temp;
        //canvas.transform.position = halfWayVector;

        // Calculate the canvas position to the left of the NPC relative to the player's perspective
        Vector3 directionToNPC = npcTransform.position - playerTransform.position;
        directionToNPC.y = 0; // Ignore vertical difference (optional, depending on your game)
        directionToNPC.Normalize();

        //// Calculate the left direction relative to the player's perspective
        Vector3 leftDirection = -Vector3.Cross(directionToNPC, Vector3.up).normalized;

        //// Set the canvas position
        canvas.transform.position = npcTransform.position + leftDirection * offset.magnitude;

        DisplayDialogue();
    }

    // This will display the dialogue box and set up intial information. Dialogue lines + choices when applicable.
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
        audioManager.PlayDialogue(currentDialogueLine.dialogueAudio); // PLAYS SPEECH AUDIO.
    }

    // Sets up the panel to display choice buttons when applicable. Also passes in relevant data into those buttons.
    private void SetupChoicePanel()
    {
        choiceButtonList.Clear();// clear the list of buttons.

        // Clear the choice buttons from before from scene
        foreach (Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Generate the buttons and passes in related button information (text, etc.)
        for (int i = 0; i < currentDialogueLine.choices.Count; i++)
        {
            DialogueElementsScript.ChoiceElement choice = currentDialogueLine.choices[i];
            GameObject button = Instantiate(choiceButtonPrefab, choicePanel.transform);
            
            ButtonDataScript buttonDataScript = button.GetComponent<ButtonDataScript>(); // Button data script is a component within each prefab button that will data related to that button.

            buttonDataScript.textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonDataScript.englishText = choice.englishChoice;
            buttonDataScript.pinyinText = choice.pinyinChoice;
            buttonDataScript.chineseText = choice.chineseChoice;
            buttonDataScript.choiceAudio = choice.choiceAudio;
            buttonDataScript.nextNode = currentDialogueLine.nextElementID[i];
            buttonDataScript.ChangeDisplayText(currentLanguageID);

            choiceButtonList.Add(button); // add it to a list so it can change later.   
        }

        choicePanel.SetActive(true); // show it.
    }




    // ------------------------- BUTTON PRESS LISTENERS ------------------------------ //
    public void ContinueButtonPressed() // public to be accessible by button assignment in inspector
    {
        canvas.SetActive(true);
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
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        speechPanelManager.DisplaySpeechPanel(clickedButton.GetComponent<ButtonDataScript>());
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

    public void ReplayButtonPressed()
    {
        audioManager.PlayDialogue(currentDialogueLine.dialogueAudio);
    }



    // ------------------------ END DIALOGUE ---------------------- //

    public void EndDialogue()
    {
        // reset visibilty of everything.
        canvas.SetActive(false);
        dialoguePanel.SetActive(false);
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

}
