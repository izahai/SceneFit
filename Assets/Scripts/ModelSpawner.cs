using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ModelSpawner : MonoBehaviour
{
    public static ModelSpawner Instance;
    public Vector3 spawnOffset = Vector3.zero;
    public string glbBaseFolder = "Avatars/";
    private GameObject currentActiveModel;
    private AllMethodsResponse lastResponse;
    private Vector3 lastpos;
    private Quaternion lastRos;
    private List<List<string>> nameModel;

    private void Awake() => Instance = this;

    public void UpdateResponse(AllMethodsResponse response, Vector3 position, Quaternion rotation)
    {
        if (response == null) return;

        lastResponse = response;
        lastpos = position;
        lastRos = rotation;

        // 1. Organize the data into our 2D list: nameModel[MethodIndex][ModelIndex]
        nameModel = new List<List<string>>
        {
            ExtractNames(response.imageEdit),
            ExtractNames(response.vlm),
            ExtractNames(response.clip),
            ExtractNames(response.aesthetic)
        };
    }

    // Helper to extract string names from ClothingResult lists
    private List<string> ExtractNames(List<ClothingResult> results)
    {
        List<string> names = new List<string>();
        if (results != null)
        {
            foreach (var item in results)
            {
                // Assuming the .glb filename corresponds to the 'name' field
                names.Add(item.name);
            }
        }
        return names;
    }

    public void SpawnModel(int iM, int iA)
    {
        if (currentActiveModel != null) Destroy(currentActiveModel);
        
        string glbFileName = nameModel[iM][iA];
        string fullPath = Path.Combine(glbBaseFolder, glbFileName + ".glb");

        currentActiveModel = new GameObject($"{glbFileName}");
        currentActiveModel.transform.SetPositionAndRotation(lastpos+spawnOffset, lastRos);

        LocalGlbLoader loader = currentActiveModel.AddComponent<LocalGlbLoader>();
        loader.Init(fullPath);
    }
}