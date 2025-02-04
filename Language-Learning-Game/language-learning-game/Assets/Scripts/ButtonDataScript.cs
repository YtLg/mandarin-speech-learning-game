using TMPro;
using UnityEngine;

public class ButtonDataScript : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string englishText;
    public string pinyinText;
    public string chineseText;
    public int nextNode;

    public void ChangeDisplayText(int languageID)
    {
        Debug.Log("IN CHANGE DISPLAY TEXT--------------------------------------");
        Debug.Log(textComponent.text);
        Debug.Log(languageID);
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
