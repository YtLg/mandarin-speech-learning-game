using System.Threading;
using UnityEngine;

public class ObjectToggleCanvas : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ObjectPanelManager objectPanelManager;
    public ObjectDataScript objectDataScript;
    public DialogueManager dialogueManager;
    public SpeechPanelManager speechPanelManager;
    public OutputPanelManager outputPanelManager;
    public PracticePanelInputManager practicePanelInputManager;
    public PracticePanelOutputManager practicePanelOutputManager;
    public Vector3 offset = new(0f, 0f, 0f);
    public Vector3 magnitudeOffset = new(0f, 0f, 0f);
    public float timerDuration = 0f;


    bool held;
    bool letGo;

    private float countdownValue;
    private Vector3 originalPosition;
    private Vector3 originalRotation;
    private void Start()
    {
        countdownValue = timerDuration;
        originalPosition = transform.position;
        originalRotation = transform.eulerAngles;
        held = false;
        letGo = false;
    }
    
    // Displays the objectPanel if the object has been interacted with.
    public void ToggleCanvas()  
    {
        held = true;
        letGo = false;
        dialogueManager.dialoguePanel.SetActive(false);
        dialogueManager.choicePanel.SetActive(false);
        speechPanelManager.dialoguePanel.SetActive(false);
        outputPanelManager.outputPanel.SetActive(false);
        practicePanelInputManager.practicePanel.SetActive(false);
        practicePanelOutputManager.practicePanelOutput.SetActive(false);
        objectPanelManager.ShowCanvas(this.transform, offset, magnitudeOffset, objectDataScript);
    }

    // used to track if it's being held or not, being referenced from inspector.
    public void LetGo()
    {
        held = false;
        letGo = true;
    }

    // Timeout timer function.
    public void FixedUpdate()
    {
        if (held == false && letGo == true)
        {
          countdownValue -= Time.deltaTime;
        }
       
        if (countdownValue <= 0f)
        {
            returnToPosition();
        }
    }

    // Makes it so the object return to its original position after being grabbed or flung, after a timeout.
    private void returnToPosition()
    {
        transform.position = originalPosition;
        transform.eulerAngles = originalRotation;
        countdownValue = timerDuration;
        held = false;
        letGo = false;
    }
}
