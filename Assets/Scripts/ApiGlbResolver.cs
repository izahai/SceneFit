using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

class AcceptAllCerts : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}

public class ApiGlbResolver : MonoBehaviour
{
    private string serverUrl =
        // "https://proconciliation-tien-erythemal.ngrok-free.dev/api/v1/all-methods";
        // "https://nondepressed-semipneumatically-eveline.ngrok-free.dev/api/v1/retrieval/all-methods";
        "http://127.0.0.1:8000/mock-api";

    [Header("GLB Mapping")]
    public string glbFolder = "Avatars";
    public string glbExtension = ".glb";

    private int requestTimeout = 300;

    public IEnumerator ResolveTopGlbsFromImage(string imagePath, System.Action<AllMethodsResponse> onResult)
    {
        byte[] imageData = File.ReadAllBytes(imagePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, Path.GetFileName(imagePath), "image/png");

        using (UnityWebRequest request = UnityWebRequest.Post(serverUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Server error: " + request.error);
                onResult?.Invoke(null);
            }
            else
            {
                // JsonUtility will now work because our models match the JSON structure
                AllMethodsResponse response = JsonUtility.FromJson<AllMethodsResponse>(request.downloadHandler.text);
                onResult?.Invoke(response);
            }
        }
    }
}
