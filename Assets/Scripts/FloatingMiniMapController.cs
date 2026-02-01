using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleMapUI : MonoBehaviour
{
    public GameObject mapCanvas;
    public InputActionProperty showButton;

    void Update()
    {
        if (showButton.action != null && showButton.action.WasPressedThisFrame())
        {
            if (mapCanvas == null)
                return;

            mapCanvas.SetActive(!mapCanvas.activeSelf);
        }
    }
}