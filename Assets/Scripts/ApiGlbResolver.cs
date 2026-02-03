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
        "https://nondepressed-semipneumatically-eveline.ngrok-free.dev/api/v1/retrieval/all-methods";
        //"http://127.0.0.1:8000/mock-api";

    [Header("GLB Mapping")]
    public string glbFolder = "Avatars";
    public string glbExtension = ".glb";

    private int requestTimeout = 300;

    /// <summary>
    /// Resolve TOP-K GLB candidates from an input image.
    /// </summary>
    /// <param name="imagePath">Absolute path to image</param>
    /// <param name="topK">How many baselines to return</param>
    /// <param name="onResult">Callback with list of GLB filenames</param>
    public IEnumerator ResolveTopGlbsFromImage(
        string imagePath,
        int topK,
        System.Action<List<string>> onResult
    )
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError($"Image not found: {imagePath}");
            onResult?.Invoke(null);
            yield break;
        }

        byte[] imageData = File.ReadAllBytes(imagePath);

        WWWForm form = new WWWForm();
        form.AddBinaryData(
            "image",
            imageData,
            Path.GetFileName(imagePath),
            "image/png"
        );

        using (UnityWebRequest request = UnityWebRequest.Post(serverUrl, form))
        {
            request.certificateHandler = new AcceptAllCerts();
            request.timeout = requestTimeout;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Server error: {request.error}");
                onResult?.Invoke(null);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log("Server response: " + json);

            List<string> glbResults = new List<string>();
            glbResults = ParseAllMethodsResponse(json);

            onResult?.Invoke(glbResults);
        }
    }

    private List<string> ParseAllMethodsResponse(string json)
    {
        List<string> glbResults = new List<string>();
        AllMethodsResponse response = JsonUtility.FromJson<AllMethodsResponse>(json);
        if (response == null)
            return glbResults; 

        AddTopResult(response.imageEdit, glbResults);
        AddTopResult(response.vlm, glbResults);
        AddTopResult(response.clip, glbResults);
        AddTopResult(response.aesthetic, glbResults);

        return glbResults;
    }

    private string BuildGlbName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
        {
            return null;
        }

        if (rawName.EndsWith(glbExtension, StringComparison.OrdinalIgnoreCase))
        {
            return rawName;
        }

        string baseName = Path.GetFileNameWithoutExtension(rawName);
        return baseName + glbExtension;
    }

    private void AddTopResult(
        List<ClothingResult> results,
        List<string> glbResults
    )
    {
        if (results == null || results.Count == 0)
            return;

        // Optional: pick highest score
        ClothingResult best = results[0];
        for (int i = 1; i < results.Count; i++)
        {
            if (results[i].score > best.score)
                best = results[i];
        }

        string glbName = BuildGlbName(best.name);
        if (!string.IsNullOrEmpty(glbName))
            glbResults.Add(glbName);
    }

}
