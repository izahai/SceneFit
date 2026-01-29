using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public GameObject menu;
    public Transform head;
    public float spawnDistance = 2f;
    public InputActionProperty showButton;
    // Update is called once per frame
    void Update()
    {
        if (showButton.action != null && showButton.action.WasPressedThisFrame())
        {
            if (menu == null || head == null)
                return;

            menu.SetActive(!menu.activeSelf);
        }

        // While the menu is active, keep it positioned in front of the head each frame
        if (menu != null && menu.activeSelf && head != null)
        {
            Vector3 forwardFlat = new Vector3(head.forward.x, 0f, head.forward.z).normalized;
            menu.transform.position = head.position + forwardFlat * spawnDistance;

            // Face the player horizontally
            menu.transform.LookAt(new Vector3(head.position.x, menu.transform.position.y, head.position.z));
            menu.transform.forward *= -1;
        }
    }
}
