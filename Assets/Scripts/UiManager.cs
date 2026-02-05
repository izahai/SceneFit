using UnityEngine;
using UnityEngine.InputSystem;

public class UiManager : MonoBehaviour
{
    [SerializeField] private GameObject ui;
    [SerializeField] private InputActionProperty trigger;
    void OnEnable()
    {
        trigger.action.Enable();
    }
    void OnDisable()
    {
        trigger.action.Disable();
    }
    void Awake()
    {
        ui.SetActive(true);
    }
    void Update()
    {
        if (trigger.action != null && trigger.action.WasPressedThisFrame())
        {
            if (ui == null)
                return;
            ui.SetActive(!ui.activeSelf);
        }
    }
}
