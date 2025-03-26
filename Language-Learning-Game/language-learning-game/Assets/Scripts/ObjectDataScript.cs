using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewObjectData", menuName = "Dialogue System/New ObjectData")]
public class ObjectDataScript : ScriptableObject
{
    public ObjectData objectData;

    [System.Serializable]
    public class ObjectData
    {
        public string englishText;           // Translations of the same text
        public string pinyinText;
        public string chineseText;
        public AudioClip objectAudio;
    }
}