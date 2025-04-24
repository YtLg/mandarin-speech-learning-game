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
    [HideInInspector] public bool isRecording = false;

    //--- Other Components ---//
    public ApiManager apiManager;

    public void StartRecording() // Begins recording the default microphone.
    {
        isRecording = true;
        recordedSpeech = Microphone.Start(null, false, maxRecLength, 44100);
    }

    // Stops the recording and returns the byte array of the audio, and the audio clip.
    public (byte[], AudioClip) StopRecording() // Ends the recording and does post-recording processing.  
    {
        Debug.Log("Ended!");
        isRecording = false;
        Debug.Log("Is recording is" + isRecording);
        var position = Microphone.GetPosition(null);
        Microphone.End(null);

        // Format the recording data correctly || 1. get the relevant audio data for converstion, 2. properly define it as a WAV file manually.
        var samples = new float[position * recordedSpeech.channels];
        recordedSpeech.GetData(samples, 0);

        AudioClip audioClip = AudioClip.Create("recording", position, recordedSpeech.channels, recordedSpeech.frequency, false);
        audioClip.SetData(samples, 0);


        bytes = WriteToWav(samples, recordedSpeech.frequency, recordedSpeech.channels);
        
        return (bytes, recordedSpeech);  
    }

    // Sends the recorded audio to the API and returns the data.
    async public Task<WhisperData> SendRecordedAudio(byte[] recordedAudio)
    {
        var result = await apiManager.SendAudio(recordedAudio);
        WhisperData data = JsonConvert.DeserializeObject<WhisperData>(result);

        await apiManager.SaveAudio((data.transcription + "Whisper"), recordedAudio);


        return (data);
    }
    
    // Sends reference + recording audio data to API for analysis + feedback.   
    async public Task<AnalysisData> SendAnalysisAudio(TranscriptButtonData transcriptButtonData, byte[]bytes)    
    {
        float[] samples1 = new float[transcriptButtonData.textAudio.samples * transcriptButtonData.textAudio.channels];
        transcriptButtonData.textAudio.GetData(samples1, 0);

        byte[] wavData = WriteToWav(samples1, transcriptButtonData.textAudio.frequency, transcriptButtonData.textAudio.channels);

        var result = await apiManager.AnalyseAudio(transcriptButtonData.startTime, transcriptButtonData.endTime, wavData, bytes);


        await apiManager.SaveAudio((transcriptButtonData.buttonTextData + "Parsel"), bytes);

        AnalysisData analysisData = JsonConvert.DeserializeObject<AnalysisData>(result);
        Debug.Log($"ParselData: {JsonConvert.SerializeObject(analysisData, Formatting.Indented)}");
        return analysisData;
    }


    // USED TO PROCESS THE REFERENCE SPEECH DATA AND GET A TRANSCRIPT BACK
    async public Task<WhisperData> SendReferenceAudio(AudioClip audio)
    {

        float[] samples = new float[audio.samples * audio.channels];
        audio.GetData(samples, 0);

        byte[] wavData = WriteToWav(samples, audio.frequency, audio.channels);

        var result = await apiManager.SendAudio(wavData);
        WhisperData data = JsonConvert.DeserializeObject<WhisperData>(result);
        Debug.Log($"WhisperData: {JsonConvert.SerializeObject(data, Formatting.Indented)}");
        return data;
    }

    // Sends the relevant information to the API, to evaluate score of a character for the output
    async public Task<ScoreData> SendScoreEvaluation(float startRec, float endRec, float startRef, float endRef, AudioClip recAudio, AudioClip refAudio)
    {
        float[] samplesRec = new float[recAudio.samples * recAudio.channels];
        recAudio.GetData(samplesRec, 0);
        byte[] wavDataRec = WriteToWav(samplesRec, recAudio.frequency, recAudio.channels);

        float[] samplesRef = new float[refAudio.samples * refAudio.channels];
        refAudio.GetData(samplesRef, 0);
        byte[] wavDataRef = WriteToWav(samplesRef, refAudio.frequency, refAudio.channels);

        var result = await apiManager.GetScore(startRec, endRec, startRef, endRef, wavDataRec, wavDataRef);
        if (result == null){ // Timestmps provided by whisperX are sometimes wrong and will result in a null comparison due to white noise, in that case this will return an automatic pass.
            ScoreData scoreData = new ScoreData();
            scoreData.score = 70;
            return scoreData;
        }
        ScoreData score = JsonConvert.DeserializeObject<ScoreData>(result);
        return score;
    }

    // Converts Unity AudioClip default format into .WAV format for API
    private byte[] WriteToWav(float[] sampleArray, int speechFrequency, int recordingChannels)
    {
        using var memoryStream = new MemoryStream(44 + sampleArray.Length * 2);
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


// DATA CLASSES - USED TO CONVERT JSON INTO A READABLE UNITY CLASS FORMAT.

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
public class AnalysisData
{
    public float[] pitchRecording;
    public float[] timestampsRecording;
    public float[] pitchReference;
    public float[] timestampsReference;
    public float accuracyScore;
    public float[] relevantDeviations;
    public string[] feedbackList;
}

public class ScoreData
{
    public float score;
}

//public class DifferenceData
//{
//    public int index_Rec;
//    public int index_Ref;
//    public float deviation;
//}




