// using UnityEngine;

// public class SelectionManager : MonoBehaviour
// {
//     public static SelectionManager Instance;
//     public string glbBaseFolder = "Avatars/";

//     private void Awake() => Instance = this;

//     public void SelectModel(ClothingResult data)
//     {
//         Debug.Log($"Loading GLB Model: {data.name}");
        
//         // Logic to load GLB from Resources or StreamingAssets
//         string fullPath = glbBaseFolder + data.name; 
        
//         // Example: If using a GLB loader library
//         // GlbLoader.Load(fullPath); 
//     }
// }