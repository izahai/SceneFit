using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnGlbOnAction : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Vector3 spawnOffset = Vector3.zero;
    public GameObject loadingPlaceholder;
    public int maxCandidates = 3;

    [Header("References")]
    public Transform characterRoot; // XR Origin / Player / Camera Rig
    public PlayerImageCapture imageCapture;
    public ApiGlbResolver serverResolver;
    public FloatingPanelController floatingPanel;


    // Cached spawn transform
    private Vector3 cachedSpawnPosition;
    private Quaternion cachedSpawnRotation;

    private List<string> resolvedGlbs = new List<string>();
    private int currentGlbIndex = -1;
    private readonly List<GameObject> loadedGlbObjects = new List<GameObject>();

    private void Awake()
    {
        if (floatingPanel != null)
        {
            floatingPanel.NextClicked += ShowNextResolvedGlb;
        }
    }

    private void OnDestroy()
    {
        if (floatingPanel != null)
        {
            floatingPanel.NextClicked -= ShowNextResolvedGlb;
        }
    }


    IEnumerator CaptureAndSpawnFromServer()
    {
        if (floatingPanel != null)
            floatingPanel.SetPanelVisible(false);

        yield return null;

        ShowLoadingPlaceholder();

        // 1. Capture image
        string imagePath = imageCapture.CaptureImage();

        // 2. Send image to server
        List<string> results = null;

        yield return StartCoroutine(
            serverResolver.ResolveTopGlbsFromImage(
                imagePath,
                maxCandidates,
                glbNames => results = glbNames
            )
        );

        resolvedGlbs.Clear();
        if (results != null && results.Count > 0)
            resolvedGlbs = new List<string>(results);

        if (resolvedGlbs.Count == 0)
        {
            Debug.LogWarning("No GLB returned from server.");
            currentGlbIndex = -1;
            yield break;
        }

        // 3. Load GLBs
        ClearLoadedGlbs();
        LoadAllGlbs();

        currentGlbIndex = 0;
        UpdateVisibleGlb();

        HideLoadingPlaceholder();
    }

    void ShowNextResolvedGlb()
    {
        if (resolvedGlbs.Count == 0)
            return;

        currentGlbIndex = (currentGlbIndex + 1) % resolvedGlbs.Count;
        UpdateVisibleGlb();
    }

    private void ShowLoadingPlaceholder()
    {
        if (loadingPlaceholder == null)
            return;

        Vector3 placeholderPosition = cachedSpawnPosition;
        placeholderPosition.y += 1.5f;

        loadingPlaceholder.transform.SetPositionAndRotation(
            placeholderPosition,
            cachedSpawnRotation
        );

        loadingPlaceholder.transform.Rotate(0f, 90f, 90f, Space.Self);
        loadingPlaceholder.SetActive(true);
    }

    private void HideLoadingPlaceholder()
    {
        if (loadingPlaceholder != null)
            loadingPlaceholder.SetActive(false);
    }

    private void LoadAllGlbs()
    {
        for (int i = 0; i < resolvedGlbs.Count; i++)
        {
            string glbFileName = resolvedGlbs[i];

            GameObject glbObject = new GameObject($"GLB_{glbFileName}");
            glbObject.transform.SetPositionAndRotation(
                cachedSpawnPosition,
                cachedSpawnRotation
            );

            LocalGlbLoader loader = glbObject.AddComponent<LocalGlbLoader>();
            loader.Init($"Avatars/{glbFileName}");
            loader.DefaultVisible = i == 0;

            loadedGlbObjects.Add(glbObject);
        }
    }

    private void UpdateVisibleGlb()
    {
        for (int i = 0; i < loadedGlbObjects.Count; i++)
        {
            GameObject glbObject = loadedGlbObjects[i];
            if (glbObject == null)
                continue;

            LocalGlbLoader loader = glbObject.GetComponent<LocalGlbLoader>();
            if (loader != null && loader.AvatarRoot != null)
            {
                loader.AvatarRoot.SetActive(i == currentGlbIndex);
            }
        }
    }

    public void TriggerCapture()
    {
        if (characterRoot == null)
        {
            Debug.LogWarning("Character Root not assigned.");
            return;
        }

        cachedSpawnPosition = characterRoot.position + spawnOffset;
        cachedSpawnRotation = characterRoot.rotation;

        StartCoroutine(CaptureAndSpawnFromServer());
    }


    private void ClearLoadedGlbs()
    {
        foreach (var obj in loadedGlbObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        loadedGlbObjects.Clear();
    }
}