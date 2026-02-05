using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour 
{
    public ApiGlbResolver apiResolver;
    public ItemSlot[] slots; // Assign your 5 slots here
    private AllMethodsResponse lastResponse;
    private int currentMethodIndex = 0;
    private string[] methodKeys = { "Image Edit", "Vision Language Model", "CLIP Model", "Aesthetic Predictor" };

    public void RequestNewImages(string path) 
    {
        StartCoroutine(apiResolver.ResolveTopGlbsFromImage(path, (response) => {
            if(response != null) {
                lastResponse = response;
                currentMethodIndex = 0;
                DisplayCurrentMethod();
            }
        }));
    }

    public void NextMethod()
    {
        if (lastResponse == null) return;
        currentMethodIndex = (currentMethodIndex + 1) % methodKeys.Length;
        DisplayCurrentMethod();
    }

    private void DisplayCurrentMethod()
    {
        List<ClothingResult> currentList = GetListByIndex(currentMethodIndex);

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < currentList.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Setup(currentList[i]);
                StartCoroutine(DownloadImage(currentList[i].image_url, slots[i].displayImage));
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
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

    private System.Collections.IEnumerator DownloadImage(string url, RawImage target) {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url)) {
            yield return request.SendWebRequest();
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success) {
                target.texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
            }
        }
    }
}