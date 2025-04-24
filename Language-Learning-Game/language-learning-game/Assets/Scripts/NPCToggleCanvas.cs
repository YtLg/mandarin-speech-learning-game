using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class NPCToggleCanvas : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public ObjectPanelManager objectPanelManager;
    public SpeechPanelManager speechPanelManager;
    public OutputPanelManager outputPanelManager;
    public PracticePanelInputManager practicePanelInputManager;
    public PracticePanelOutputManager practicePanelOutputManager;

    public DialogueElementsScript npcDialogueElements;
    public Vector3 offset = new(0f, 0f, 0f);
    public Vector3 offset4Mag = new(0f, 0f, 0f);


    // When the VR headset grabs the object, it will call dialogue manager to set up the dialogue and pass in the associated NPC's dialogue script
    // and also resets the visibility of the UI anels.
    public void ToggleCanvas()
    {
        dialogueManager.ShowCanvas(gameObject, transform, offset, offset4Mag, npcDialogueElements);
        objectPanelManager.objectPanel.SetActive(false);
        speechPanelManager.dialoguePanel.SetActive(false);
        outputPanelManager.outputPanel.SetActive(false);
        practicePanelInputManager.practicePanel.SetActive(false);
        practicePanelOutputManager.practicePanelOutput.SetActive(false);
    }

}

