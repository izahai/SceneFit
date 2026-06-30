using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class ApiAnalyticsReporter : MonoBehaviour
{
    public string analyticsUrl = "http://127.0.0.1:8000/gallery-feedback";
    private int requestTimeout = 30;

    public IEnumerator PostGalleryFeedback(GalleryFeedbackPayload payload, Action<bool> onDone)
    {
        if (payload == null)
        {
            onDone?.Invoke(false);
            yield break;
        }

        string json = JsonUtility.ToJson(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(analyticsUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = requestTimeout;

            yield return request.SendWebRequest();

            bool success = request.result == UnityWebRequest.Result.Success;
            if (!success)
            {
                Debug.LogError("Analytics post failed: " + request.error);
            }

            onDone?.Invoke(success);
        }
    }
}
