using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImageCaptureRequester : MonoBehaviour
{
    [Header("Settings")]
    public Vector3 spawnOffset = Vector3.zero;

    [Header("References")]
    public Transform characterRoot;
    public PlayerImageCapture imageCapture;
    public ApiGlbResolver serverResolver;
    public GameObject loadingPlaceholder;

    public void TriggerCapture()
    {
        Vector3 spawnPos = characterRoot.position + spawnOffset;
        Quaternion spawnRot = characterRoot.rotation;
        StartCoroutine(ProcessCaptureRoutine(spawnPos, spawnRot));
    }

    private IEnumerator ProcessCaptureRoutine(Vector3 pos, Quaternion rot)
    {
        if (loadingPlaceholder) {
            loadingPlaceholder.SetActive(true);
            loadingPlaceholder.transform.SetPositionAndRotation(pos, rot);
            loadingPlaceholder.transform.Rotate(0f, 90f, 90f, Space.Self);
        }

        // 1. Capture & Resolve
        string path = imageCapture.CaptureImage();
        AllMethodsResponse results = null;

        yield return StartCoroutine(serverResolver.ResolveTopGlbsFromImage(path, (res) => results = res));

        if (results == null) {
            if (loadingPlaceholder) loadingPlaceholder.SetActive(false);
            yield break;
        }

        // 2. Serialize data
        GalleryManager.Instance.UpdateResponse(results);
        
        // 3. Tell the Spawner to handle the 3D work
        ModelSpawner.Instance.UpdateResponse(results, pos, rot);
        
        // currentGlbIndex = 0;
        if (loadingPlaceholder) loadingPlaceholder.SetActive(false);
    }
}