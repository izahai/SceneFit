using System;
using System.Collections.Generic;

[Serializable]
public class ClothingResult
{
    public string name;
    public float score;
    public string image_url;
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
public class GalleryMethodFeedback
{
    public string methodId;
    public int selectedRank;
    public int[] viewCounts;
}

[Serializable]
public class GalleryFeedbackPayload
{
    public List<GalleryMethodFeedback> responses;
    public string finalWinnerMethodId;
}

[Serializable]
public class TranscriptResponse
{
    public string transcript;
}
