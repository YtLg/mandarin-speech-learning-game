using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutputPanelManager : MonoBehaviour
{
    //--- Audio Player Reference ---//
    public AudioManager audioManager;

    //--- REFERENCE TO OTHER PANELS---//
    public SpeechPanelManager speechPanelManager;
    public DialogueManager dialoguePanelManager;
    public PracticePanelManager practicePanelManager;


    //----- DATA ------ //
    private WhisperData recordingData;
    private AudioClip recSpeech;
    private WhisperData idealData;
    private AudioClip choiceAudio;

    //----- UI DISPLAY ELEMENTS------//
    public GameObject outputPanel;
    public GameObject textButtonArea;
    public TextMeshProUGUI transcriptText;

    //----- UI INTERACTIBLE ELEMENTS --------//
    public Button continueButton;
    public Button tryAgainButton;
    public Button hearRecButton;
    public Button hearExButton;

    //--- PREFAB TRANSCRIPTION BUTTON COMPONENTS---//
    public GameObject transcriptionArea;
    public GameObject transcriptButtonPrefab;
    private List<GameObject> transcriptButtonList = new List<GameObject>(); // in case we add translation functionality.

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outputPanel.SetActive(false);

        // Set up button listeners
        continueButton.onClick.AddListener(ContinuePressed);
        tryAgainButton.onClick.AddListener(TryAgainPressed);
        hearRecButton.onClick.AddListener(WhatYouSaidPressed);
        hearExButton.onClick.AddListener(WhatTheySaidPressed);

    }

    //--- UI ELEMENT SETUP & LOGIC ---//

    // Sets up the UI elements of the result panel
    // data = transcript of speech input / referenceData = transcript of fluent speaker audio / buttonData = fluent speaker audio file / 
    public void SetupDisplayOutputPanel(WhisperData data, AudioClip recordedSpeech, WhisperData referenceData, AudioClip referenceAudio)
    {
        recordingData = data;
        choiceAudio = referenceAudio;
        recSpeech = recordedSpeech;
        idealData = referenceData;
        transcriptText.text = ("You Said : \"" + data.transcription + "\"");
        SetupExpectedTextButtons();
        outputPanel.SetActive(true);
    }

    // sets up the button display of the expected speech text
    //void SetupExpectedTextButtons()
    //{
    //    transcriptButtonList.Clear();
    //    float expectedTotal = 0;
    //    float actualTotal = 0;

    //    // Clear the choice buttons from before from scene
    //    foreach (Transform child in transcriptionArea.transform)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    int recWordCount = recordingData.words.Count;
    //    for (int i = 0; i < idealData.words.Count; i++) {
    //    {
    //        // NOTE: listener of prefab buttons are defined within transcriptbuttondata
    //        WordData idealWord = idealData.words[i];

    //        GameObject button = Instantiate(transcriptButtonPrefab, transcriptionArea.transform);
    //        TranscriptButtonData transcriptButtonData = button.GetComponent<TranscriptButtonData>(); // Button data script is a component within each prefab button that will data related to that button.
    //        transcriptButtonData.textComponent = button.GetComponentInChildren<TextMeshProUGUI>(); // can't do this in prefab,  must be done during runtime

    //        transcriptButtonData.buttonTextData = idealWord.word;
    //        transcriptButtonData.expectedProb = idealWord.probability;
    //        expectedTotal += transcriptButtonData.expectedProb;
    //        transcriptButtonData.startTime = idealWord.start;
    //        transcriptButtonData.endTime = idealWord.end;
    //        transcriptButtonData.actualProb = 0.0f; // default for user input doesn't match.
    //        string idealTemp = CleanMandarinText(idealWord.word);

    //        // if idealWord.words[i] = 0 , avoid checking left neighbor
    //        // if idealWord.words[i] = Count, avoid checking right neighbor.
    //        // Check if i is equal to, or larger than recWordCount, or if i = 0.

    //        // if idealWord.words[i] = 0, check if recWordCount size is 1. If it is, do some logic to check recordingData.words[i]. Check if recWordCount is at least 2. If it is, check recordingData.words[i+1]
    //        // if idealWord.words[i] = idealData.words.Count, check if recWordCount size is = i. If it is, check recordingData.words[i], then check recWordCount[i-1]. If it isn't, check if recWordCount size is = i-1. If it is, check recWordCount[i-1], otherwise, do nothing.
    //        // if idealWord.words[i] isn't any of the above. Check if recWordCount is <= i, if it is, check current and previous recordingData.words[i] & [i-1], then check if recWordCount<= i+1, if it is, check, if isn't leave it. If recWordCount <= i, check If recWordCount <= i-1, if it is, check i-1, if it isn't, do nothing.




    //            foreach (WordData recordingWord in recordingData.words) // if it was said, replace 0.0f with actual prob.
    //        {
    //            string recTemp = CleanMandarinText(recordingWord.word); // TEMPORARY LOGIC TO DECIDE INITIAL PRONUNCIATION ACCURACY.
    //            if (recTemp.Contains(idealTemp) || idealTemp.Contains(recTemp)) // BANDAID FOR ISSUSE WITH WORD SEGMENTATION FROM STT.
    //            {
    //                {
    //                    transcriptButtonData.actualProb = recordingWord.probability; 
    //                    actualTotal+= transcriptButtonData.actualProb;
    //                }
    //            }
    //            transcriptButtonData.SetText();// SETS THE DISPLAY TEXT + TEXT COLOUR DEPENDING ON EXP PROB / ACTUAL PROB PERCENTAGE.
    //            transcriptButtonList.Add(button);
    //        }
    //        if( ((actualTotal/expectedTotal)*100) < 65){
    //            continueButton.gameObject.SetActive(false);
    //        }
    //        else
    //        {
    //            continueButton.gameObject.SetActive(true);
    //        }
    //    }
    //}

        // ---- Button Press Listeners ---- //
        public void TranscriptButtonPressed(GameObject buttonPressed)
        {
            
            outputPanel.SetActive(false);
            practicePanelManager.SetupPanel(buttonPressed.GetComponent<TranscriptButtonData>(), choiceAudio);
        }

        void TryAgainPressed()
        {
            outputPanel.gameObject.SetActive(false);
            speechPanelManager.ShowSpeechPanel();
            // TODO: Call some function in SpeechPanelManager to make it appear again, then hide this panel.
        }

        void ContinuePressed()            
        {
            outputPanel.gameObject.SetActive(false);
            dialoguePanelManager.ContinueButtonPressed();
            // TODO: Call some function in DialogueManager to make it go to the next dialogue then hide this panel.
        }

        // Plays the player speech input or example audio depending on button pressed.
        void WhatYouSaidPressed()
        {
            audioManager.PlayDialogue(recSpeech);
        }

        void WhatTheySaidPressed()
        {
            audioManager.PlayDialogue(choiceAudio);
        }

        //--- HELPER FUNCTIONS ---//

        public string CleanMandarinText(string text) // removes punctuation and spaces for comparative purposes.
        {
            string pattern = @"[，。？！、；：“”‘’（）《》【】…—·\sA-Za-z]";
            string cleanedText = Regex.Replace(text, pattern, "");
            return cleanedText;
        }

}
