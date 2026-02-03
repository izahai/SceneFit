using UnityEngine;
using UnityEngine.InputSystem;

public class CaptureHotkeyTest : MonoBehaviour
{
    public PlayerImageCapture capture;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            var path = capture.CaptureImage();
            Debug.Log("Capture returned path: " + path);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            if (!string.IsNullOrEmpty(path))
            {
                Application.OpenURL("file://" + path);
            }
#endif
        }
    }
}
