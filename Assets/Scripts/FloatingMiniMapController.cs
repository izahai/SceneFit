using UnityEngine;

public class ToggleMapUI : MonoBehaviour
{
    public GameObject mapCanvas;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            mapCanvas.SetActive(!mapCanvas.activeSelf);
        }
    }
}