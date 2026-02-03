using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private GameObject minimapCanvas;
    [Header("Toggle Button")]
    [SerializeField] private InputActionProperty showButton;
    // Update is called once per frame
    void OnEnable()
    {
        showButton.action.Enable();
    }

    void OnDisable()
    {
        showButton.action.Disable();
    }
    void Awake()
    {
        minimapCanvas.SetActive(false);
    }
    void Update()
    {
        if (showButton.action != null && showButton.action.WasPressedThisFrame())
        {
            if (minimapCanvas == null)
                return;
            minimapCanvas.SetActive(!minimapCanvas.activeSelf);
        }
    }
}
