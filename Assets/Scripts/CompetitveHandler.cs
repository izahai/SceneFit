using UnityEngine;
using System.Collections.Generic;

public class CompetitveHandler : MonoBehaviour
{
    public static CompetitveHandler Instance;
    public List<CandidateOutfit> cans = new List<CandidateOutfit>();

    private void Awake() 
    {
        if (Instance == null) Instance = this;
    }

    public void AppendCandidate(int iM, int iA)
    {
        CandidateOutfit newOutfit = new CandidateOutfit 
        { 
            indexMethod = iM, 
            indexAvatar = iA 
        };
        
        cans.Add(newOutfit);
    }
}