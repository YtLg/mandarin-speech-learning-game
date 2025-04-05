using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ObjectDataScript;

public class ObjectPanelManager : MonoBehaviour
{
    // UI Display Elements ---
    public GameObject canvas;
    public GameObject objectPanel;
    public TextMeshProUGUI objectText;

    // iNTERACTIBLES ------
    public Button translateButton;
    public Button practiceButton;
    public Button doneButton;
    public Button replayButton;
    
    // Object References -----
    public SpeechPanelManager speechPanelManager;
    public AudioManager audioManager;
    public Transform playerTransform;

    // Data -------
    int currentLanguageID = 0;
    ObjectData currentObjectData;

    void Start()
    {
        canvas.SetActive(false);
        objectPanel.SetActive(false);
        translateButton.onClick.AddListener(TranslateButtonPressed);
        replayButton.onClick.AddListener(ReplayButtonPressed);
        practiceButton.onClick.AddListener(PracticeButtonPressed);
        doneButton.onClick.AddListener(DoneButtonPressed);

    }

    public void ShowCanvas(Transform objectTransform, Vector3 offset, Vector3 magnitudeOffset, ObjectDataScript objectData)
    {
        currentLanguageID = 0;
        currentObjectData = objectData.objectData;
        canvas.SetActive(true);

        canvas.transform.position = playerTransform.position + playerTransform.forward * 2;
        canvas.transform.position += offset;

        DisplayName();
    }

    private void DisplayName()
    {
        objectPanel.SetActive(true);
        ChangeObjectText(currentLanguageID);
        audioManager.PlayDialogue(currentObjectData.objectAudio);
    }

    public void ShowObjectPanel()
    {
        objectPanel.SetActive(true);
    }

    // ------- BUTTON LISTENERS -------------

    public void PracticeButtonPressed()
    {
        objectPanel.SetActive(false);
        speechPanelManager.DisplaySpeechPanel(ObjectDataToButtonData(currentObjectData), 1);
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

        ChangeObjectText(currentLanguageID);
    }

    public void DoneButtonPressed()
    {
        canvas.SetActive(false);
        objectPanel.SetActive(false);
        currentObjectData = null;
    }

    public void ReplayButtonPressed()
    {
        audioManager.PlayDialogue(currentObjectData.objectAudio);
    }


    // HELPER FUNCTIONS ------------------

    void ChangeObjectText(int langID)
    {
        if (langID == 0)
        {
            objectText.text = currentObjectData.chineseText;
        }
        else if (langID == 1)
        {
            objectText.text = currentObjectData.pinyinText;
        }
        else
        {
            objectText.text = currentObjectData.englishText;
        }

    }

    // Not the most elegant fix, but simplifies refactoring process of code to work for both object + NPC UI display.
    ButtonDataScript ObjectDataToButtonData(ObjectData objectData)
    {
        ButtonDataScript buttonDataScript = new ButtonDataScript();
        buttonDataScript.textComponent = null;
        buttonDataScript.englishText = objectData.englishText;
        buttonDataScript.pinyinText = objectData.pinyinText;
        buttonDataScript.chineseText = objectData.chineseText;
        buttonDataScript.choiceAudio = objectData.objectAudio;

        return buttonDataScript;
    }
}
