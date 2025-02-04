using TMPro;
using UnityEngine;
using System.Collections.Generic; // needed for list DON'T DELETE
using UnityEngine.UI;


public class DialogueManager : MonoBehaviour
{
    // DIALOGUE ELEMENTS //
    public GameObject dialogueCanvas;
    public GameObject dialoguePanel;
    public GameObject choicePanel;

    public Transform playerTransform;

    private TextMeshProUGUI speakerNameText;
    private TextMeshProUGUI dialogueText;

    public Button continueButton;
    private Button exitButton;

    // DIALOGUE DATA STORE //
    private DialogueElementsScript currentDialogue;
    private DialogueElementsScript.DialogueElementList currentDialogueLine;
    private int currentElementIndex;


    // CHOICE DATA STORE //
    public GameObject choiceButtonPrefab;
    private List<GameObject> choiceButtonList = new List<GameObject>();

    int currentLanguageID; // 0 = English | 1 = PinYin | 2 = Chinese


    // ---- initial setup --- //
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Set up dialogue text values
        speakerNameText = dialoguePanel.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        dialogueText = dialoguePanel.transform.Find("Dialogue").GetComponent<TextMeshProUGUI>();
    }

    public void ShowCanvas(Transform npcTransform, Vector3 offset, DialogueElementsScript dialogueElements)
    {
        currentLanguageID = 0;
        dialogueCanvas.SetActive(true); // Show the canvas
        currentElementIndex = 0; // Start at 0
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
        ChangeDialogueLanguage(currentLanguageID);
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

    public void ChoiceSelected() // same reasoning as before for public.
    {
        // Move onto the next dialogue. (For now.)

        // Maybe add clearning the choice buttons here?
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

        ChangeDialogueLanguage(currentLanguageID);

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


    // ------------------------ END DIALOGUE ---------------------- //

    public void EndDialogue()
    {
        dialogueCanvas.SetActive(false);
        dialoguePanel.SetActive(false);
        currentDialogue = null;
    }


    //--------------------------------------------------------------------------------------------------------------------------//

    // UTILITY FUNCTIONS // 

    public void ChangeDialogueLanguage(int langID)
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
