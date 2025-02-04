using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/New Dialogue")]
    public class DialogueElementsScript : ScriptableObject
    {
        public string npcName; // Name of the NPC, only one per file since it shouldn't change.
        public List<DialogueElementList> dialogueElementList; // Creates a list in the inspector called Dialogue elements

        public class ChoiceElement
        {
            public string englishChoice;
            public string pinyinChoice;
            public string chineseChoice;
        }

        [System.Serializable]
        public class DialogueElementList // Each item of that list will be a class containing various fields.
        {
            public string englishText;           // English Dialogue
            public string pinyinText;            // Pinyin
            public string chineseText;           // Chinese
            public bool hasChoice;               // If the dialogue has choices
            public List<string> choices;         // List of choices
            public List<int> nextElementID;    // Indices of next dialogue nodes
        } // So whenever someone adds a new dialogue line, it will have all the relevant information for the manager to use to integrate logic.
        
        
        
    }