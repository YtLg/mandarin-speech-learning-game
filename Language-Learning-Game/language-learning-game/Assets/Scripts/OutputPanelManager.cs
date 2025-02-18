using TMPro;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.LudiqRootObjectEditor;
using static UnityEngine.Rendering.DebugUI.Table;

public class OutputPanelManager : MonoBehaviour
{
    // DATA ------
    private WhisperData whisperData;
    private ButtonDataScript buttonTextData;

    // UI DISPLAY ELEMENTS------
    public GameObject outputPanel;
    public GameObject textButtonArea;
    public TextMeshProUGUI transcriptText;

    // UI INTERACTIBLE ELEMENTS --------
    public Button ContinueButton;
    public Button TryAgainButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outputPanel.SetActive(false);
    }


    // Sets up the UI elements of the result panel
    void setupDisplayOutputPanel(WhisperData data, ButtonDataScript buttonData)
    {
        whisperData = data;
        buttonTextData = buttonData;
        transcriptText.text = data.transcription;
        outputPanel.SetActive(true);
    }

    public void transcriptButtonPressed(GameObject buttonPressed)
    {

    
    }


    // sets up the button display of the expected speech text
    void setupExpectedTextButtons()
    {
        ///<summary>
        ///1. Send example audio through whisper to get transcript.
        ///2.Send speech audio data through whisper to get transcript.
        ///3.Send both to Class displayPanel();
        ///4.Display each word in example audio transcript words[] as an individual button.
        ///5.Colour them by how present speech audio transcript words are in the example transcript data
        ///NOTE: You have to implement audio into implementation
        /// </summary>

    }
}
