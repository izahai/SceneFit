using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GalleryManager : MonoBehaviour 
{
    public static GalleryManager Instance;
    public TextMeshProUGUI methodTitleText;
    public ItemSlot[] slots; // Assign your 5 slots here
    private AllMethodsResponse lastResponse;
    private int currentMethodIndex = 0;
    private string[] methodTitles = { "Image Edit", "Vision Language Model", "CLIP Model", "Aesthetic Predictor" };
    private void Awake() => Instance = this;



    public void UpdateResponse(AllMethodsResponse res) 
    {
        lastResponse = res;
        currentMethodIndex = 0;
        DisplayCurrentMethod();
    }

    public void NextMethod()
    {
        if (lastResponse == null) return;
        currentMethodIndex = (currentMethodIndex + 1) % methodTitles.Length;
        DisplayCurrentMethod();
    }

    private void DisplayCurrentMethod()
    {
        if (methodTitleText != null) methodTitleText.text = methodTitles[currentMethodIndex];
        
        List<ClothingResult> currentList = GetListByIndex(currentMethodIndex);

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
}