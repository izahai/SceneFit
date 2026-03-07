using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public RawImage displayImage;
    public Color onColor = Color.red;
    public Color offColor = Color.white;

    private Button avaButton;
    private Image toggleBackground;
    private int indexMethod, indexAvatar;
    [HideInInspector] public ClothingResult currentData;
    [HideInInspector] public Toggle likeToggle; 
    
    void Awake()
    {
        // Fallback: If you forgot to drag them in the inspector, find them by code
        if (likeToggle == null) 
            likeToggle = GetComponentInChildren<Toggle>();

        if (toggleBackground == null && likeToggle != null)
            toggleBackground = likeToggle.transform.Find("Background").GetComponent<Image>();

        if (avaButton == null)
            avaButton = GetComponentInChildren<Button>();
    }

    void Start()
    {
        if (likeToggle != null)
        {
            likeToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) 
                {
                    // Tell the manager this slot was selected
                    GalleryManager.Instance.OnSlotSelected(this);
                    UpdateColor(true);
                }
                else 
                {
                    UpdateColor(false);
                }
            });
            avaButton.onClick.AddListener(() => {
                UserStudyLogger.Instance.RecordView(indexMethod, indexAvatar);
                ModelSpawner.Instance.SpawnModel(indexMethod, indexAvatar);
            });

        }
    }

    void UpdateColor(bool isOn)
    {
        if (toggleBackground != null)
            toggleBackground.color = isOn ? onColor : offColor;
    }

    public void Setup(ClothingResult data, int iM, int iAva)
    {
        currentData = data;
        indexMethod = iM;
        indexAvatar = iAva;
        if (likeToggle != null) likeToggle.isOn = false; 
    }
}