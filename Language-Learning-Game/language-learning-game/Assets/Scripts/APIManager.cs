using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.Sentis;
using System.Globalization;
using Unity.VisualScripting;

public class ApiManager : MonoBehaviour
{
    private readonly string urlWhisper = "http://127.0.0.1:8000"; // No modifying the URL outside of editor.
    private readonly string urlParsel = "http://127.0.0.1:8001";

    // SENDS AND RECIEVES REQUESTS TO/FROM WHISPER API
    public async Task<string> SendAudio(byte[] audioData)
    {
        WWWForm form = new();
        form.AddBinaryData("file", audioData, "recorded_audio.wav", "audio/wav");
        using UnityWebRequest request = UnityWebRequest.Post(urlWhisper + "/transcribeAudio", form);
        var operation = request.SendWebRequest();

        await operation;

        if (request.result == UnityWebRequest.Result.Success)
        {
            string result = request.downloadHandler.text;
            Debug.Log("Transcription: " + result);
            return result;
        }
        else
        {
            Debug.LogError($"Error: {request.error}, Status Code: {request.responseCode}");
            return null;
        }
    }
    public async Task<string> AnalyseAudio(float start, float end, byte[] reference, byte[] recording)
    {
        string url = $"{urlParsel}/analyseAudio"+ $"?timestampStart={UnityWebRequest.EscapeURL(start.ToString(CultureInfo.InvariantCulture))}"
            + $"&timestampEnd={UnityWebRequest.EscapeURL(end.ToString(CultureInfo.InvariantCulture))}";
        WWWForm form = new();
        //form.AddField("timestampStart", start.ToString(CultureInfo.InvariantCulture));
        //form.AddField("timestampEnd", end.ToString(CultureInfo.InvariantCulture));
        form.AddBinaryData("referenceFile", reference, "reference_audio.wav", "audio/wav");
        form.AddBinaryData("recordingFile", recording, "recording_audio.wav", "audio/wav");
        
        using UnityWebRequest request = UnityWebRequest.Post(url, form);
        var operation = request.SendWebRequest();

        await operation;
        if (request.result == UnityWebRequest.Result.Success)
        {
            string result = request.downloadHandler.text;
            Debug.Log("Result: " + result);
            return result;
        }
        else
        {
            Debug.LogError($"Error: {request.error}, Status Code: {request.responseCode}");
            Debug.LogError($"Server Response: {request.downloadHandler.text}");
            return null;
        }
    }


    public async Task<string> TranslateCharacter(string character)
    {
        string url = $"{urlWhisper}/getTranslations" + $"?character={UnityWebRequest.EscapeURL(character)}";
        WWWForm form = new();

        using UnityWebRequest request = UnityWebRequest.Post(url, form);
        var operation = request.SendWebRequest();

        await operation;
        if (request.result == UnityWebRequest.Result.Success)
        {
            string result = request.downloadHandler.text;
            Debug.Log("Result: " + result);
            return result;
        }
        else
        {
            Debug.LogError($"Error: {request.error}, Status Code: {request.responseCode}");
            Debug.LogError($"Server Response: {request.downloadHandler.text}");
            return null;
        }
    }

    // SENDS AND RECIEVES REQUESTS TO/FROM PARSELMOUTH API
}
