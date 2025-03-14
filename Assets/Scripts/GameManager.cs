using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class GameManager : MonoBehaviour
{
    public World currentWorld;
    public static GameManager Instance { get; private set; }

    public List<MergeProgression> progressionPaths;

    public Dictionary<TileBase, TileBase> mergeProgression = new Dictionary<TileBase, TileBase>();

    public PlayerInventory inventory;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        InitializeMergreProgression();
        inventory.InitializePlayerInventory(6, 12, 16, 64);
    }

    void InitializeMergreProgression()
    {
        foreach (var path in progressionPaths)
        {
            foreach (var pair in path.mergePairs)
            {
                mergeProgression[pair.currentItem] = pair.nextItem;
            }

        }
        
    }

}
