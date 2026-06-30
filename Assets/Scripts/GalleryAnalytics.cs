using UnityEngine;
using System.Collections.Generic;

public class GalleryAnalytics : MonoBehaviour
{
    public static GalleryAnalytics Instance;

    [HideInInspector]
    public string[] methodIds = { "Image Editing", "Vision Language Model", "CLIP Model", "Asthetic Model" };

    private int[] selectedRanks;
    private int[][] viewCounts;

    private void Awake() => Instance = this;

    public void ResetSession(int methodCount, int slotCount)
    {
        selectedRanks = new int[methodCount];
        for (int i = 0; i < selectedRanks.Length; i++) selectedRanks[i] = -1;

        viewCounts = new int[methodCount][];
        for (int i = 0; i < methodCount; i++)
        {
            viewCounts[i] = new int[slotCount];
        }
    }

    public void RecordView(int methodIndex, int slotIndex)
    {
        if (viewCounts == null) return;
        if (methodIndex < 0 || methodIndex >= viewCounts.Length) return;
        if (slotIndex < 0 || slotIndex >= viewCounts[methodIndex].Length) return;

        viewCounts[methodIndex][slotIndex] += 1;
    }

    public void SetSelectedRank(int methodIndex, int selectedIndex)
    {
        if (selectedRanks == null) return;
        if (methodIndex < 0 || methodIndex >= selectedRanks.Length) return;

        selectedRanks[methodIndex] = selectedIndex;
    }

    public string GetMethodId(int methodIndex)
    {
        if (methodIndex < 0) return string.Empty;
        if (methodIds != null && methodIndex < methodIds.Length) return methodIds[methodIndex];
        return methodIndex.ToString();
    }

    public GalleryFeedbackPayload BuildPayload(string finalWinnerMethodId)
    {
        var payload = new GalleryFeedbackPayload
        {
            responses = new List<GalleryMethodFeedback>(),
            finalWinnerMethodId = finalWinnerMethodId ?? string.Empty
        };

        if (selectedRanks == null || viewCounts == null) return payload;

        int methodCount = selectedRanks.Length;
        for (int i = 0; i < methodCount; i++)
        {
            payload.responses.Add(new GalleryMethodFeedback
            {
                methodId = GetMethodId(i),
                selectedRank = selectedRanks[i],
                viewCounts = viewCounts[i]
            });
        }

        return payload;
    }
}
