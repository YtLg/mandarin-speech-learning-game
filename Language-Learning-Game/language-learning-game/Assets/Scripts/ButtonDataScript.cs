using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDataScript : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string englishText;
    public string pinyinText;
    public string chineseText;
    public int nextNode;

    private DialogueManager dialogueManager;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        button.onClick.AddListener(() => dialogueManager.ChoiceSelected(gameObject));
    }

    public void ChangeDisplayText(int languageID)
    {
        //Debug.Log("IN CHANGE DISPLAY TEXT--------------------------------------");
        //Debug.Log(textComponent.text);
        //Debug.Log(languageID);
        if (languageID == 0)
        {
            textComponent.text = englishText;
        }
        else if(languageID == 1) {
            textComponent.text = pinyinText;
        }
        else
        {
            textComponent.text = chineseText;
        }
    }
}
