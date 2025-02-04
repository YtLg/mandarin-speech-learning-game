using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class DialogueManager : MonoBehaviour
{
    // DIALOGUE ELEMENTS //
    public GameObject dialogueCanvas;
    public GameObject dialoguePanel;
    public GameObject choicePanel;

    private TextMeshProUGUI speakerNameText;
    private TextMeshProUGUI dialogueText;

    private Button translateButton;
    private Button replayButton;

    private Button continueButton;
    private Button exitButton;

    // DIALOGUE DATA STORE //
    private DialogueElementsScript currentDialogue;
    private DialogueElementsScript.DialogueElementList currentDialogueLine;
    private int currentElementIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Set up dialogue text values
        speakerNameText = dialoguePanel.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        dialogueText = dialoguePanel.transform.Find("Dialogue").GetComponent<TextMeshProUGUI>();
    }

    public void ShowCanvas(Transform npcTransform, Vector3 offset, DialogueElementsScript dialogueElements)
    {
        dialogueCanvas.SetActive(true);
        currentElementIndex = 0;
        currentDialogue = dialogueElements; // pass it in so other can use it without having it to pass
        currentDialogueLine = currentDialogue.dialogueElementList[currentElementIndex];

        dialogueCanvas.transform.position = npcTransform.position + offset;
        DisplayDialogue();
        
    }

    private void DisplayDialogue()
    {

        speakerNameText.text = currentDialogue.name;
        dialogueText.text = currentDialogueLine.englishText;

    }

    public void ContinueButtonPressed()
    {
        Debug.Log("current dialogue " + currentDialogue.dialogueElementList.Count);
        Debug.Log("next element list count " + currentDialogueLine.nextElementID.Count);
        if (currentDialogue == null || currentDialogueLine.nextElementID.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentElementIndex = currentDialogueLine.nextElementID[0];
        currentDialogueLine = currentDialogue.dialogueElementList[currentElementIndex];
        DisplayDialogue();
    }

    public void EndDialogue()
    {
        dialogueCanvas.SetActive(false);
        currentDialogue = null;
    }

}
