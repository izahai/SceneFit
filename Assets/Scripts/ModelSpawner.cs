using UnityEngine;
using System.Collections.Generic;

public class ModelSpawner : MonoBehaviour
{
    public static ModelSpawner Instance;
    public string glbBaseFolder = "Avatars/";
    private List<GameObject> loadedGlbObjects = new List<GameObject>();

    private void Awake() => Instance = this;

    public void SpawnResolvedModels(List<string> glbFileNames, Vector3 position, Quaternion rotation)
    {
        ClearCurrentModels();

        for (int i = 0; i < glbFileNames.Count; i++)
        {
            GameObject glbObject = new GameObject($"GLB_{glbFileNames[i]}");
            glbObject.transform.SetPositionAndRotation(position, rotation);

            // Logic to load the actual model
            LocalGlbLoader loader = glbObject.AddComponent<LocalGlbLoader>();
            loader.Init($"{glbBaseFolder}{glbFileNames[i]}");
            
            // Only show the first one by default
            loader.DefaultVisible = (i == 0);

            loadedGlbObjects.Add(glbObject);
        }
    }

    public void SetVisibleIndex(int index)
    {
        for (int i = 0; i < loadedGlbObjects.Count; i++)
        {
            if (loadedGlbObjects[i] == null) continue;
            
            var loader = loadedGlbObjects[i].GetComponent<LocalGlbLoader>();
            if (loader != null && loader.AvatarRoot != null)
            {
                loader.AvatarRoot.SetActive(i == index);
            }
        }
    }

    public void ClearCurrentModels()
    {
        foreach (var obj in loadedGlbObjects) if (obj != null) Destroy(obj);
        loadedGlbObjects.Clear();
    }
}