using TMPro;
using UnityEngine;
using System.IO;
using HuggingFace.API;
using System.Collections.Generic;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine.Networking;

public class SpeechManager : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public DialogueManager dialogueManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            EndRecording();
        }
    }

    public void StartRecording()
    {
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;
    }
    public void EndRecording()
    {
        var position = Microphone.GetPosition(null);
        Microphone.End(null);
        var samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;
        SendRecording();

    }

    private void SendRecording()
    {
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response =>
        {
            dialogueManager.RecordingFinished(response);
            displayText.text = "you have said: " + response;
        }, error => {
            dialogueManager.RecordingFinished(error);
            displayText.text =  error;
        });
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }
}
