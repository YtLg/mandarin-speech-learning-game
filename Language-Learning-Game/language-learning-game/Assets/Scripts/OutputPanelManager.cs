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
    void SetupExpectedTextButtons()
    {
        transcriptButtonList.Clear();

        // Clear the choice buttons from before from scene
        foreach (Transform child in transcriptionArea.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (WordData idealWord in idealData.words)
        {
            // NOTE: listener of prefab buttons are defined within transcriptbuttondata

            GameObject button = Instantiate(transcriptButtonPrefab, transcriptionArea.transform);
            TranscriptButtonData transcriptButtonData = button.GetComponent<TranscriptButtonData>(); // Button data script is a component within each prefab button that will data related to that button.
            transcriptButtonData.textComponent = button.GetComponentInChildren<TextMeshProUGUI>(); // can't do this in prefab,  must be done during runtime

            transcriptButtonData.buttonTextData = idealWord.word;
            transcriptButtonData.expectedProb = idealWord.probability;
            transcriptButtonData.startTime = idealWord.start;
            transcriptButtonData.endTime = idealWord.end;
            transcriptButtonData.actualProb = 0.0f; // default for user input doesn't match.
            string idealTemp = CleanMandarinText(idealWord.word);
            foreach (WordData recordingWord in recordingData.words) // if it was said, replace 0.0f with actual prob.
            {
                string recTemp = CleanMandarinText(recordingWord.word);
                if (recTemp.Contains(idealTemp) || idealTemp.Contains(recTemp))
                {
                    {
                        transcriptButtonData.actualProb = recordingWord.probability;
                    }
                }
                transcriptButtonData.SetText();
                transcriptButtonList.Add(button);
            }

            ///<summary>
            ///1. Send example audio through whisper to get transcript. /
            ///2.Send speech audio data through whisper to get transcript. /
            ///3.Send both to Class displayOutputPanel(); /
            ///4.Display each word in example audio transcript words[] as an individual button. /
            ///5.Colour them by how present speech audio transcript words are in the example transcript data /
            ///NOTE: You have to implement audio into implementation /
            ///ALSO: SAVE 
            /// </summary>

        }
    }

        // ---- Button Press Listeners ----
        public void TranscriptButtonPressed(GameObject buttonPressed)
        {

        }

        void TryAgainPressed()
        {
            // Call some function in SpeechPanelManager to make it appear again, then hide this panel.
        }

        void ContinuePressed()
        {
            // Call some function in DialogueManager to make it go to the next dialogue then hide this panel.
        }

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
