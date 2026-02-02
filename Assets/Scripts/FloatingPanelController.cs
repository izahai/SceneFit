using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class FloatingPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI panelTextTMP;

    [Header("Input Actions (VR)")]
    [SerializeField] private InputActionProperty togglePanelAction;

    [Header("Panel Texts")]
    [SerializeField] private string[] texts =
    {
        "Caption Matching",
        "Image Matching",
        "Tournament Selection"
    };

    private int currentIndex;

    public event System.Action NextClicked;

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        UpdateText();
        SetPanelVisible(panelRoot != null && panelRoot.activeSelf);
    }

    private void OnEnable()
    {
        if (togglePanelAction.action != null)
            togglePanelAction.action.Enable();
    }

    private void OnDisable()
    {
        if (togglePanelAction.action != null)
            togglePanelAction.action.Disable();
    }

    private void Update()
    {
        // VR controller toggle input
        if (togglePanelAction.action != null &&
            togglePanelAction.action.WasPressedThisFrame())
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(!panelRoot.activeSelf);
    }

    private void OnNextButtonClicked()
    {
        ShowNextText();
        NextClicked?.Invoke();
    }

    private void ShowNextText()
    {
        if (texts == null || texts.Length == 0)
            return;

        currentIndex = (currentIndex + 1) % texts.Length;
        UpdateText();
    }

    private void UpdateText()
    {
        if (panelTextTMP == null || texts.Length == 0)
            return;

        panelTextTMP.text = texts[currentIndex];
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelRoot != null)
            panelRoot.SetActive(visible);
    }
}