using System;
using System.Collections.Generic;

// [Serializable]
// public class ClothesResponse
// {
//     public string[] query;
//     public List<ClothingResult> results;
// }

[Serializable]
public class ClothingResult
{
    public string name;
    public float score;
}

[Serializable]
public class AllMethodsResponse
{
    public List<ClothingResult> imageEdit;
    public List<ClothingResult> vlm;
    public List<ClothingResult> clip;
    public List<ClothingResult> aesthetic;
}

[Serializable]
public class TranscriptResponse
{
    public string transcript;
}