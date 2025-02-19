using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SpeechInputManager : MonoBehaviour
{

    //--- Data Store ---//
    public int maxRecLength = 10;
    private AudioClip recordedSpeech;
    private byte[] bytes;
    private byte[] referenceBytes;
    [HideInInspector] public bool isRecording = false;
    private string result;

    //--- Other Components ---//
    public ApiManager apiManager;

    public void StartRecording() // Begins recording the default microphone.
    {
        isRecording = true;
        recordedSpeech = Microphone.Start(null, false, maxRecLength, 44100);
    }

    async public Task<(WhisperData, AudioClip)> StopRecording() // Ends the recording and does post-recording processing.  
    {
        Debug.Log("Ended!");
        isRecording = false;
        var position = Microphone.GetPosition(null);
        Microphone.End(null);

        var samples = new float[position * recordedSpeech.channels];
        recordedSpeech.GetData(samples, 0);

        AudioClip audioClip = AudioClip.Create("recording", position, recordedSpeech.channels, recordedSpeech.frequency, false);
        audioClip.SetData(samples, 0);


        bytes = WriteToWav(samples, recordedSpeech.frequency, recordedSpeech.channels);

        var result = await apiManager.SendAudio(bytes);
        WhisperData data = JsonConvert.DeserializeObject<WhisperData>(result);

        Debug.Log("Transription is:" + data.transcription);

        foreach (WordData word in data.words)
        {
            Debug.Log($"Word: {word.word}, Start: {word.start}, End: {word.end}, Probability: {word.probability}");
        }

        return (data, recordedSpeech);
    }


    // USED TO PROCESS THE REFERENCE SPEECH DATA AND GET A TRANSCRIPT BACK
    async public Task<WhisperData> SendReferenceAudio(AudioClip audio)
    {

        float[] samples = new float[audio.samples * audio.channels];
        audio.GetData(samples, 0);

        byte[] wavData = WriteToWav(samples, audio.frequency, audio.channels);

        var result = await apiManager.SendAudio(wavData);
        WhisperData data = JsonConvert.DeserializeObject<WhisperData>(result);

        Debug.Log("Transcription is: " + data.transcription);

        foreach (WordData word in data.words)
        {
            Debug.Log($"Word: {word.word}, Start: {word.start}, End: {word.end}, Probability: {word.probability}");
        }

        return data;
    }


    // Converts Unity AudioClip default format into .WAV format for API
    private byte[] WriteToWav(float[] sampleArray, int speechFrequency, int recordingChannels)
    {
        using (var memoryStream = new MemoryStream(44 + sampleArray.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                // Converts it to the correct header for WAV.
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + sampleArray.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)recordingChannels);
                writer.Write(speechFrequency);
                writer.Write(speechFrequency * recordingChannels * 2);
                writer.Write((ushort)(recordingChannels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(sampleArray.Length * 2);

                foreach (var sample in sampleArray)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }
}

public class WhisperData
{
    public string transcription = "";
    public List<WordData> words;
}

[Serializable]
public class WordData
{
    public string word;
    public float start;
    public float end;
    public float probability;
}
