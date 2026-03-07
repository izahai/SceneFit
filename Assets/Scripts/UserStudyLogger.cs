using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class UserStudyLogger : MonoBehaviour
{
    public static UserStudyLogger Instance;
    [SerializeField] private string studyApiUrl = "http://127.0.0.1:8000/api/v1/study";

    private int[][] viewCounts;
    private string[][] imgURLs;
    private string[] selectedURLs;
    private int slotCount = 5;
    private string[] methodNames = { "Image Editing", "Vision Language Model", "CLIP Model", "Asthetic Model" };

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
        for (int i = 0; i < 4; i++)
        {
            viewCounts[i] = new int[slotCount];
            imgURLs[i] = new string[slotCount];
        }
    }

    /// <summary>Call from GalleryManager.DisplayCurrentMethod to record the image URLs shown for a method.</summary>
    public void SetImgURLs(int methodIndex, List<string> urls)
    {
        if (methodIndex < 0 || methodIndex >= 4) return;
        for (int i = 0; i < slotCount; i++)
            imgURLs[methodIndex][i] = (i < urls.Count) ? urls[i] : "";
    }

    /// <summary>Call from GalleryManager.NextMethod to record which URL the user selected for a method.</summary>
    public void SetSelectedURL(int methodIndex, int selectedSlotIndex)
    {
        if (methodIndex < 0 || methodIndex >= 4) return;
        selectedURLs[methodIndex] = (selectedSlotIndex >= 0 && selectedSlotIndex < slotCount)
            ? imgURLs[methodIndex][selectedSlotIndex]
            : "";
    }

    /// <summary>Call from ItemSlot when user clicks the avatar button to preview a model.</summary>
    public void RecordView(int methodIndex, int slotIndex)
    {
        if (methodIndex >= 0 && methodIndex < 4 && slotIndex >= 0 && slotIndex < slotCount)
            viewCounts[methodIndex][slotIndex]++;
    }

    public void Submit(int finalWinnerMethodIndex)
    {
        StartCoroutine(PostStudyResponse(finalWinnerMethodIndex));
    }

    private IEnumerator PostStudyResponse(int finalWinnerMethodIndex)
    {
        var sb = new StringBuilder();
        sb.Append("{\"responses\":[");

        for (int m = 0; m < 4; m++)
        {
            sb.Append("{");
            sb.Append($"\"methodName\":\"{Esc(methodNames[m])}\",");

            // imgURLs
            sb.Append("\"imgURLs\":[");
            for (int s = 0; s < slotCount; s++)
            {
                sb.Append($"\"{Esc(imgURLs[m][s])}\"");
                if (s < slotCount - 1) sb.Append(",");
            }
            sb.Append("],");

            // selectedURL
            sb.Append($"\"selectedURL\":\"{Esc(selectedURLs[m])}\",");

            // viewCounts
            sb.Append("\"viewCounts\":[");
            for (int s = 0; s < slotCount; s++)
            {
                sb.Append(viewCounts[m][s]);
                if (s < slotCount - 1) sb.Append(",");
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
                Debug.Log($"[UserStudy] OK: {request.downloadHandler.text}");
            else
                Debug.LogError($"[UserStudy] Error: {request.error}");
        }
    }

    private static string Esc(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}