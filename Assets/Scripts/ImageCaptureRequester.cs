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

    private List<string> resolvedGlbs = new List<string>();
    private int currentGlbIndex = -1;

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
        
        // 2. Tell the Spawner to handle the 3D work
        // resolvedGlbs = results;
        // ModelSpawner.Instance.SpawnResolvedModels(resolvedGlbs, pos, rot);
        
        // currentGlbIndex = 0;
        if (loadingPlaceholder) loadingPlaceholder.SetActive(false);
    }

    public void CycleNext()
    {
        if (resolvedGlbs.Count == 0) return;
        currentGlbIndex = (currentGlbIndex + 1) % resolvedGlbs.Count;
        ModelSpawner.Instance.SetVisibleIndex(currentGlbIndex);
    }
}