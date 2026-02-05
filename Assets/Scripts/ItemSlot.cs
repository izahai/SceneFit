using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public RawImage displayImage;
    public Toggle likeToggle;
    
    [HideInInspector] public ClothingResult currentData;

    public void Setup(ClothingResult data)
    {
        currentData = data;
        likeToggle.isOn = false; // Reset for new page
    }

    // public void OnClickSelect()
    // {
    //     SelectionManager.Instance.SelectModel(currentData);
    // }

    // public void OnToggleLike(bool isLiked)
    // {
    //     UserFeedbackManager.Instance.RecordFeedback(currentData, currentMethod, isLiked);
    // }
}