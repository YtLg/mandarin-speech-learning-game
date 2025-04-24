using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Globalization;

public class ApiManager : MonoBehaviour
{
    // Internal test PC IP
    private readonly string urlWhisper = "http://127.0.0.1:8000"; // No modifying the URL outside of editor.
    private readonly string urlParsel = "http://127.0.0.1:8001";

    // Local Network IP
    //private readonly string urlWhisper = "http://192.168.225.215:8000"; // No modifying the URL outside of editor.
    //private readonly string urlParsel = "http://192.168.225.215:8001";


    // SENDS AND RECIEVES REQUESTS TO/FROM WHISPER API
    public async Task<string> SendAudio(byte[] audioData)
    {
        Debug.Log("Entering API manager for whisper");
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


    // Sends the input and reference to the audio analysis pipeline and returns the out9p9ut9.9
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

    // saves the audio for user study purposes.
    public async Task SaveAudio(string filename, byte[] file)
    {
        string url = $"{urlWhisper}/saveAudio?filename={UnityWebRequest.EscapeURL(filename)}";
        WWWForm form = new();
        form.AddBinaryData("file", file, "audio.wav", "audio/wav");

        using UnityWebRequest request = UnityWebRequest.Post(url, form);
        var operation = request.SendWebRequest();
        await operation;

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("Audio saved successfully!");
        }
    }


    // Used to get the score of each button prefab generated after speaking a phrase.
    public async Task<string> GetScore(float startRec, float endRec, float startRef, float endRef, byte[] recAudio, byte[] refAudio)
    {
        Debug.Log("Entering Get Score API");
        string url = $"{urlParsel}/getScore" + $"?timestampStartRec={UnityWebRequest.EscapeURL(startRec.ToString(CultureInfo.InvariantCulture))}"
            + $"&timestampEndRec={UnityWebRequest.EscapeURL(endRec.ToString(CultureInfo.InvariantCulture))}" 
            + $"&timestampStartRef={UnityWebRequest.EscapeURL(startRef.ToString(CultureInfo.InvariantCulture))}"
            + $"&timestampEndRef={UnityWebRequest.EscapeURL(endRef.ToString(CultureInfo.InvariantCulture))}";
            WWWForm form = new();
            form.AddBinaryData("recFile", recAudio, "recAudio.wav", "audio/wav");
            form.AddBinaryData("refFile", refAudio, "refAudio.wav", "audio/wav");
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

    // Used to get the pinyin and english translations of individual characters
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

}
