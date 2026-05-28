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
    [Header("HTTP")]
    [SerializeField] private string serverUrl =
        // "https://proconciliation-tien-erythemal.ngrok-free.dev/api/v1/all-methods";
        "https://synonymic-knowledgeable-edgardo.ngrok-free.dev/api/v1/retrieval/all-methods";
        // "http://127.0.0.1:8000/mock-api";
    [SerializeField] private bool useMockResponse = false;

    [Header("GLB Mapping")]
    public string glbFolder = "Avatars";
    public string glbExtension = ".glb";

    [Header("Mock Data")]
    [SerializeField] private string mockImageFileName = "avatars_0a4a89d3ac42468d8c59d9b5f7014e92.png";

    private int requestTimeout = 300;
    private readonly string[] mockBaseNames =
    {
        "avatars_0a4a89d3ac42468d8c59d9b5f7014e92",
        "avatars_mock_01",
        "avatars_mock_02",
        "avatars_mock_03",
        "avatars_mock_04"
    };

    public IEnumerator ResolveTopGlbsFromImage(string imagePath, System.Action<AllMethodsResponse> onResult)
    {
        if (useMockResponse)
        {
            Debug.Log("[ApiGlbResolver] Using mock retrieval response.");
            onResult?.Invoke(BuildMockResponse());
            yield break;
        }

        byte[] imageData = File.ReadAllBytes(imagePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageData, Path.GetFileName(imagePath), "image/png");

        using (UnityWebRequest request = UnityWebRequest.Post(serverUrl, form))
        {
            request.timeout = requestTimeout;
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

    private AllMethodsResponse BuildMockResponse()
    {
        string mockImageUrl = BuildLocalMockImageUrl();
        return new AllMethodsResponse
        {
            imageEdit = BuildMockMethodResults(mockImageUrl),
            vlm = BuildMockMethodResults(mockImageUrl),
            clip = BuildMockMethodResults(mockImageUrl),
            aesthetic = BuildMockMethodResults(mockImageUrl),
        };
    }

    private List<ClothingResult> BuildMockMethodResults(string imageUrl)
    {
        List<ClothingResult> results = new List<ClothingResult>();
        for (int i = 0; i < mockBaseNames.Length; i++)
        {
            results.Add(new ClothingResult
            {
                name = mockBaseNames[i],
                score = 0.95f - (i * 0.08f),
                image_url = imageUrl
            });
        }
        return results;
    }

    private string BuildLocalMockImageUrl()
    {
        string localPath = Path.Combine(Application.dataPath, "MyTexture", mockImageFileName);
        return new System.Uri(localPath).AbsoluteUri;
    }
}
