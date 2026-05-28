using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GalleryManager : MonoBehaviour 
{
    public static GalleryManager Instance;
    private const int FinalChoiceMethodIndex = 4;
    public TextMeshProUGUI methodTitleText;
    public ItemSlot[] slots; // Assign your 5 slots here
    private AllMethodsResponse lastResponse;
    private int currentMethodIndex = 0;
    private string[] methodTitles = { "Image Edit", "Vision Language Model", "CLIP Model", "Aesthetic Predictor", "Final Choice"};
    private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
    private void Awake() => Instance = this;

    public void UpdateResponse(AllMethodsResponse res) 
    {
        lastResponse = res;
        currentMethodIndex = 0;
        CompetitveHandler.Instance?.ResetCandidates();
        if (UserStudyLogger.Instance != null)
            UserStudyLogger.Instance.Reset();
        else
            Debug.LogWarning("[UserStudy] UserStudyLogger is missing from the scene.");
        DisplayCurrentMethod();
    }

    public void NextMethod()
    {
        if (lastResponse == null) return;

        int selectedIndex = GetSelectedIndex();
        if (selectedIndex < 0)
        {
            Debug.LogWarning("[UserStudy] Select an outfit before continuing.");
            return;
        }

        if (currentMethodIndex == FinalChoiceMethodIndex)
        {
            SubmitFinalChoice(selectedIndex);
            return;
        }

        CompetitveHandler.Instance?.AppendCandidate(currentMethodIndex, selectedIndex);
        if (UserStudyLogger.Instance != null)
            UserStudyLogger.Instance.SetSelectedURL(currentMethodIndex, selectedIndex);
        else
            Debug.LogWarning("[UserStudy] UserStudyLogger is missing from the scene.");

        if (currentMethodIndex == 3)
        {
            currentMethodIndex = FinalChoiceMethodIndex;
            DisplayFinalChoice();
            return;
        }

        currentMethodIndex = (currentMethodIndex + 1) % methodTitles.Length;
        DisplayCurrentMethod();
    }

    private void DisplayCurrentMethod()
    {
        if (methodTitleText != null) methodTitleText.text = methodTitles[currentMethodIndex];
        
        List<ClothingResult> currentList = GetListByIndex(currentMethodIndex);

        // Record image URLs for the study logger
        List<string> urls = new List<string>();
        for (int i = 0; i < currentList.Count; i++)
            urls.Add(currentList[i].image_url);
        if (UserStudyLogger.Instance != null)
            UserStudyLogger.Instance.SetImgURLs(currentMethodIndex, urls);
        else
            Debug.LogWarning("[UserStudy] UserStudyLogger is missing from the scene.");

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < currentList.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Setup(currentList[i], currentMethodIndex, i);
                StartCoroutine(DownloadImage(currentList[i].image_url, slots[i].displayImage));
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    private void DisplayFinalChoice()
    {
        if (methodTitleText != null) methodTitleText.text = methodTitles[currentMethodIndex];

        if (CompetitveHandler.Instance == null)
        {
            Debug.LogWarning("[UserStudy] CompetitveHandler is missing from the scene.");
            return;
        }

        int i;
        int finalCandidateCount = Mathf.Min(CompetitveHandler.Instance.cans.Count, slots.Length - 1);
        for (i = 0; i < finalCandidateCount; i++)
        {
            slots[i].gameObject.SetActive(true);
            CandidateOutfit can = CompetitveHandler.Instance.cans[i];
            ClothingResult data = GetListByIndex(can.indexMethod)[can.indexAvatar];
            slots[i].Setup(data, can.indexMethod, can.indexAvatar);
            StartCoroutine(DownloadImage(data.image_url, slots[i].displayImage));
        }
        for (; i < slots.Length; i++)
            slots[i].gameObject.SetActive(false);
    }

    private List<ClothingResult> GetListByIndex(int index)
    {
        return index switch {
            0 => lastResponse.imageEdit,
            1 => lastResponse.vlm,
            2 => lastResponse.clip,
            3 => lastResponse.aesthetic,
            _ => lastResponse.vlm
        };
    }

    private System.Collections.IEnumerator DownloadImage(string url, RawImage target) 
    {
        // 2. Check if we already have it in memory
        if (imageCache.ContainsKey(url))
        {
            target.texture = imageCache[url];
            yield break; // Exit the coroutine early
        }

        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url)) 
        {
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success) 
            {
                Texture2D downloadedTexture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
                
                // 3. Store it in the cache for next time
                if (!imageCache.ContainsKey(url))
                {
                    imageCache.Add(url, downloadedTexture);
                }

                target.texture = downloadedTexture;
            }
            else 
            {
                Debug.LogError("Error downloading image: " + request.error);
            }
        }
    }

    public void OnSlotSelected(ItemSlot selectedSlot)
    {
        foreach (var slot in slots)
        {
            if (slot != selectedSlot && slot.gameObject.activeSelf)
            {
                slot.GetComponentInChildren<Toggle>().isOn = false;
            }
        }
    }

    public int GetSelectedIndex()
    {
        for (int i = 0; i < slots.Length; i++)
        {   
            if (slots[i].likeToggle.isOn)
            {
                return i;
            }
        }
        
        return -1; // No selection found
    }

    private void SubmitFinalChoice(int selectedIndex)
    {
        if (CompetitveHandler.Instance == null)
        {
            Debug.LogWarning("[UserStudy] CompetitveHandler is missing from the scene.");
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= CompetitveHandler.Instance.cans.Count)
        {
            Debug.LogWarning("[UserStudy] Invalid final selection.");
            return;
        }

        if (UserStudyLogger.Instance == null)
        {
            Debug.LogWarning("[UserStudy] UserStudyLogger is missing from the scene.");
            return;
        }

        if (UserStudyLogger.Instance.IsSubmitting || UserStudyLogger.Instance.HasSubmitted)
        {
            Debug.LogWarning("[UserStudy] Submission already in progress or completed.");
            return;
        }

        int winnerMethodIndex = CompetitveHandler.Instance.cans[selectedIndex].indexMethod;
        UserStudyLogger.Instance.Submit(winnerMethodIndex);
    }
}
