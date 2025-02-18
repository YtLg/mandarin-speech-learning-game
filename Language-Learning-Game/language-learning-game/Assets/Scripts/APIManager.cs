using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

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

    // SENDS AND RECIEVES REQUESTS TO/FROM PARSELMOUTH API
}
