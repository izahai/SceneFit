using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class UserStudyLogger : MonoBehaviour
{
    public static UserStudyLogger Instance;

    [Header("HTTP")]
    [SerializeField] private string studyApiUrl = "https://synonymic-knowledgeable-edgardo.ngrok-free.dev/api/v1/study/response";

    private int[][] viewCounts;
    private string[][] imgURLs;
    private string[] selectedURLs;
    private bool isSubmitting;
    private bool hasSubmitted;
    private const int SlotCount = 5;
    private readonly string[] methodNames = { "Image Editing", "Vision Language Model", "CLIP Model", "Asthetic Model" };

    public bool IsSubmitting => isSubmitting;
    public bool HasSubmitted => hasSubmitted;

    private void Awake()
    {
        Instance = this;
        Reset();
    }

    public void Reset()
    {
        viewCounts = new int[4][];
        imgURLs = new string[4][];
        selectedURLs = new string[4];
        isSubmitting = false;
        hasSubmitted = false;
        for (int i = 0; i < 4; i++)
        {
            viewCounts[i] = new int[SlotCount];
            imgURLs[i] = new string[SlotCount];
        }
    }

    /// <summary>Call from GalleryManager.DisplayCurrentMethod to record the image URLs shown for a method.</summary>
    public void SetImgURLs(int methodIndex, List<string> urls)
    {
        if (methodIndex < 0 || methodIndex >= 4) return;
        for (int i = 0; i < SlotCount; i++)
            imgURLs[methodIndex][i] = (i < urls.Count) ? urls[i] : "";
    }

    /// <summary>Call from GalleryManager.NextMethod to record which URL the user selected for a method.</summary>
    public void SetSelectedURL(int methodIndex, int selectedSlotIndex)
    {
        if (methodIndex < 0 || methodIndex >= 4) return;
        selectedURLs[methodIndex] = (selectedSlotIndex >= 0 && selectedSlotIndex < SlotCount)
            ? imgURLs[methodIndex][selectedSlotIndex]
            : "";
    }

    /// <summary>Call from ItemSlot when user clicks the avatar button to preview a model.</summary>
    public void RecordView(int methodIndex, int slotIndex)
    {
        if (methodIndex >= 0 && methodIndex < 4 && slotIndex >= 0 && slotIndex < SlotCount)
            viewCounts[methodIndex][slotIndex]++;
    }

    public void Submit(int finalWinnerMethodIndex)
    {
        if (isSubmitting || hasSubmitted)
        {
            Debug.LogWarning("[UserStudy] Duplicate submit ignored.");
            return;
        }

        StartCoroutine(PostStudyResponse(finalWinnerMethodIndex));
    }

    private IEnumerator PostStudyResponse(int finalWinnerMethodIndex)
    {
        isSubmitting = true;

        var sb = new StringBuilder();
        sb.Append("{\"responses\":[");

        for (int m = 0; m < 4; m++)
        {
            sb.Append("{");
            sb.Append($"\"methodName\":\"{Esc(methodNames[m])}\",");

            // imgURLs
            sb.Append("\"imgURLs\":[");
            for (int s = 0; s < SlotCount; s++)
            {
                sb.Append($"\"{Esc(imgURLs[m][s])}\"");
                if (s < SlotCount - 1) sb.Append(",");
            }
            sb.Append("],");

            // selectedURL
            sb.Append($"\"selectedURL\":\"{Esc(selectedURLs[m])}\",");

            // viewCounts
            sb.Append("\"viewCounts\":[");
            for (int s = 0; s < SlotCount; s++)
            {
                sb.Append(viewCounts[m][s]);
                if (s < SlotCount - 1) sb.Append(",");
            }
            sb.Append("]}");
            if (m < 3) sb.Append(",");
        }

        // Winner
        string winnerName = (finalWinnerMethodIndex >= 0 && finalWinnerMethodIndex < methodNames.Length)
            ? methodNames[finalWinnerMethodIndex]
            : "";
        sb.Append($"],\"winnerMethodName\":\"{Esc(winnerName)}\"}}");

        string json = sb.ToString();
        Debug.Log($"[UserStudy] Submitting: {json}");

        using (UnityWebRequest request = new UnityWebRequest(studyApiUrl, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                hasSubmitted = true;
                Debug.Log($"[UserStudy] OK: {request.downloadHandler.text}");
            }
            else
                Debug.LogError($"[UserStudy] Error: {request.error}");
        }

        isSubmitting = false;
    }

    private static string Esc(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
