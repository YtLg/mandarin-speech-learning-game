using NUnit.Framework;
using System;
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
    public PracticePanelInputManager practicePanelInputManager;
    public ObjectPanelManager objectPanelManager;


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
    private List<GameObject> transcriptButtonList = new(); // in case we add translation functionality.

    //--- CALCULATION TEMP VARIABLES ---//
    float probability = 0.0f;
    int tempCount = 0;
    int objectOrNPC;

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
    public void SetupDisplayOutputPanel(WhisperData data, AudioClip recordedSpeech, WhisperData referenceData, AudioClip referenceAudio, int type)
    {
        objectOrNPC = type;
        recordingData = data;
        choiceAudio = referenceAudio;
        recSpeech = recordedSpeech;
        idealData = referenceData;
        transcriptText.text = ("You Said : \"" + data.transcription + "\"");
        SetupExpectedTextButtons();
        ContinueConditionalSetup();
        outputPanel.SetActive(true);
    }

    // sets up the button display of the expected speech text transcript
    void SetupExpectedTextButtons()
    {
        transcriptButtonList.Clear();

        // Clear the choice buttons from before from scene
        foreach (Transform child in transcriptionArea.transform)
        {
            Destroy(child.gameObject);
        }

        int recWordCount = recordingData.words.Count;
        int idealWordCount = idealData.words.Count;
        for (int i = 0; i < idealWordCount; i++)
        {
                // NOTE: listener of prefab buttons are defined within transcriptbuttondata
                WordData idealWord = idealData.words[i];
                idealWord.probability += 0.01f;


                
                GameObject button = Instantiate(transcriptButtonPrefab, transcriptionArea.transform);
                transcriptButtonList.Add(button);
                TranscriptButtonData transcriptButtonData = button.GetComponent<TranscriptButtonData>(); // Button data script is a component within each prefab button that will data related to that button.
                transcriptButtonData.textComponent = button.GetComponentInChildren<TextMeshProUGUI>(); // can't do this in prefab,  must be done during runtime
                
                transcriptButtonData.textAudio = choiceAudio;
                transcriptButtonData.buttonTextData = idealWord.word;
                transcriptButtonData.expectedProb = (float)(idealWord.probability + 0.001);
                transcriptButtonData.startTime = idealWord.start;
                transcriptButtonData.endTime = idealWord.end;
                transcriptButtonData.actualProb = 0.0f; // default for user input doesn't match.


                // LOGIC TO DETERMINE ACTUAL PROBABILITY // 
                // Each word segment will check its neighboring words for the presence of its words in that word segment.
                // If it is present then it will add it into a running total, and average it based on how many words were present to get the total.

                int count = 0;              // reset count + sum every new ideal word.
                float cumulativeSum = 0;
                int max = 0;
                if (i == 0) // 0 CASE
                {
                    if (recWordCount >= i + 2)
                    {
                        count += 1;
                        (probability, tempCount) = CheckString(idealWord, recordingData.words[i + 1]);
                        cumulativeSum += probability;
                        count += tempCount;
                    }
                    count += 1;
                    (probability, tempCount) = CheckString(idealWord, recordingData.words[i]);
                    cumulativeSum += probability;
                    count += tempCount;
                    transcriptButtonData.actualProb += cumulativeSum / count;
                }

                else if (recWordCount > 1) // i !=0 CASE
                {
                    if (recWordCount > i + 1)
                    {
                        max = 1; // before + current + next item
                    }
                    else if (recWordCount == idealWordCount)
                    {
                        max = 0; // before + current item
                    }
                    else if (recWordCount == idealWordCount - 1)
                    {
                        max = -1; // before item
                    }

                    for (int j = -1; j <= max; j++) //if recording data size is larger than current i.
                    {
                        count += 1;
                        (probability, tempCount) = CheckString(idealWord, recordingData.words[i + j]);
                        cumulativeSum += probability;
                        count += tempCount;
                    }

                    transcriptButtonData.actualProb += (float)((cumulativeSum / count) + 0.001);
                }

            transcriptButtonData.SetText();
        }
    }

    public void ContinueConditionalSetup()
    {
        float actualProbTotal = 0;
        float expectedProbTotal = 0;
        for (int i = 0; i < transcriptButtonList.Count; i++)
        {
            TranscriptButtonData transcriptButtonData = transcriptButtonList[i].GetComponent<TranscriptButtonData>(); // Button data script is a component within each prefab button that will data related to that button.
            actualProbTotal += transcriptButtonData.actualProb;
            expectedProbTotal += transcriptButtonData.expectedProb;
        }

        if (float.IsNaN(actualProbTotal))
        {
            continueButton.gameObject.SetActive(false);
            return;
        }

        float totalPercent = actualProbTotal / expectedProbTotal * 100;
        if (totalPercent < 60)
        {
            continueButton.gameObject.SetActive(false);
            return;
        }

        continueButton.gameObject.SetActive(true);
    }

    public void DisplayThePanel()
    {
        outputPanel.SetActive(true);
    }

    // ---- Button Press Listeners ---- //
    public void TranscriptButtonPressed(GameObject buttonPressed)
    {
            
        outputPanel.SetActive(false);
        practicePanelInputManager.SetupPanel(buttonPressed.GetComponent<TranscriptButtonData>());
    }

    void TryAgainPressed()
    {
        outputPanel.SetActive(false);
        speechPanelManager.ShowSpeechPanel();
        // TODO: Call some function in SpeechPanelManager to make it appear again, then hide this panel.
    }

    void ContinuePressed()            
    {
        outputPanel.SetActive(false);
        if (objectOrNPC == 0)
        {
            dialoguePanelManager.ContinueButtonPressed();
        }
        else
        {
            objectPanelManager.ShowObjectPanel();
        }
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

    public (float,int) CheckString(WordData ideal, WordData recording)
    {
    string idealText = CleanMandarinText(ideal.word);
    string recordedText = CleanMandarinText(recording.word);
        if (idealText.Contains(recordedText) || idealText.Contains(recordedText))
        {
            recording.probability += (float)0.001;
            return (recording.probability/ideal.probability, 0);
        }
        return (0, -1);
    }

}

