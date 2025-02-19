using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class NPCToggleCanvas : MonoBehaviour
{
    public GameObject canvas;
    public DialogueManager dialogueManager;
    public SpeechPanelManager speechPanelManager;
    public OutputPanelManager outputPanelManager;

    public DialogueElementsScript npcDialogueElements;
    public Vector3 offset = new(0f, 0f, 0f);


    // Update is called once per frame
    public void ToggleCanvas()
    {
        dialogueManager.ShowCanvas(transform, offset, npcDialogueElements);
        speechPanelManager.dialoguePanel.SetActive(false);
        outputPanelManager.outputPanel.SetActive(false);
    }

}

