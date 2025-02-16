using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "MergeProgression", menuName = "MergeProgression")]
[System.Serializable]
public class MergeProgression : ScriptableObject
{
    public int A = 10;
    [System.Serializable] public class MergePair
    {
        public TileBase currentItem;
        public TileBase nextItem;
    }

    public List<MergePair> mergePairs = new List<MergePair>();

}
