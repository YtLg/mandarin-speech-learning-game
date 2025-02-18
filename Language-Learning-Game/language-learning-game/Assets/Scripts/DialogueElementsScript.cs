using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/New Dialogue")]
    public class DialogueElementsScript : ScriptableObject
    {
        public string npcName; // Name of the NPC, only one per file since it shouldn't change.
        public List<DialogueElementList> dialogueElementList; // Creates a list in the inspector called Dialogue elements

        [System.Serializable]
        public class ChoiceElement
        {
            public string englishChoice; // translations of the same choice text
            public string pinyinChoice;
            public string chineseChoice;
            public AudioClip choiceAudio;
    }

        [System.Serializable]
        public class DialogueElementList // Each item of that list will be a class containing various fields.
        {
            public string englishText;           // Translations of the same text
            public string pinyinText;           
            public string chineseText;
            public AudioClip dialogueAudio;
            public bool hasChoice;               // If the dialogue has choices?
            public List<ChoiceElement> choices;         // List of choices
            public List<int> nextElementID;    // Indices of next dialogue nodes
        } // So whenever someone adds a new dialogue line, it will have all the relevant information for the manager to use to integrate logic.    
        
    }