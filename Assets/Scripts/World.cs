using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class World : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] public List<Tilemap> worldLayers;

    public TileBase floorBlock;

    private Transform placementIndicator;


    private const float C1 = 0.02f;
    private const float C2 = 0.001f;

    private TileBlock lastHighlightedTile = null;
    private TileBase pickedUpTileBase;
    private TileBlock pickedUpTileComponent;
    private Vector3 pickedUpOGPosition = Vector3.zero;
    private int pickedUpLayerIndex = -1;

    private int aimedLayerIndex = -1;


    private void Start()
    {
        Tilemap[] layers = GetComponentsInChildren<Tilemap>();
        foreach (Tilemap layer in layers)
        {
            worldLayers.Add(layer);
        }
        //worldLayers[0].BoxFill(new Vector3Int(0, 0, 0), floorBlock, 1000, 1000, -1000, -1000);
    }


    void Update()
    {
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            TileHoverOrPickUp(hit);

            // SPAWN TILES --------------------------
            if (Input.GetMouseButtonUp(1))
            {
                Tilemap aboveLayer;
                try
                {
                    aboveLayer = worldLayers[aimedLayerIndex + 1];
                }
                catch (ArgumentOutOfRangeException)
                {
                    // either add a new layer or just prevent the action
                    return;
                }
                Tilemap baseLayer = worldLayers[aimedLayerIndex];
                Vector3Int target = baseLayer.WorldToCell(hit.point);
                int tilesSpawned = SpawnTilesFromBlockRandomized(target, baseLayer, aboveLayer, 9, 6);
            }

            if (pickedUpTileComponent != null)
            {
                //placementIndicator.position = targetLayer.GetCellCenterWorld(tilePosition);
                Vector3 normal = hit.normal;

                // PICK UP ------------------------------
                if (Input.GetMouseButtonUp(0))
                {
                    Tilemap selectedLayer = worldLayers[aimedLayerIndex];
                    Tilemap pickedUpLayer = worldLayers[pickedUpLayerIndex];

                    pickedUpTileComponent.Release();
                    pickedUpLayer.SetTile(pickedUpLayer.WorldToCell(pickedUpOGPosition), null);

                    Tilemap aboveLayer;
                    try
                    {
                        aboveLayer = worldLayers[aimedLayerIndex + 1];
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // either add a new layer or just prevent the action
                        aboveLayer = null;
                    }

                    bool canBePlaced;
                    bool canBeMerged;
                    Tilemap placeLayer; 
                    Vector3Int? placePosition;

                    Vector3Int pointedTilePosition;

                    if (aimedLayerIndex == 0)
                    {
                        pointedTilePosition = selectedLayer.WorldToCell(hit.point);
                        (canBePlaced, canBeMerged, placeLayer, placePosition) =
                        pickedUpTileComponent.PlaceVerify(pointedTilePosition, normal, aboveLayer); // This should be executed at all times for the tile indicator to position itself correctly
                    }
                    else
                    {
                        pointedTilePosition = selectedLayer.WorldToCell(hit.transform.position);
                        (canBePlaced, canBeMerged, placeLayer, placePosition) =
                        pickedUpTileComponent.PlaceVerify(pointedTilePosition, normal, selectedLayer, aboveLayer); // This should be executed at all times for the tile indicator to position itself correctly
                    }

                    if (!canBePlaced)
                    {
                        pickedUpLayer.SetTile(pickedUpLayer.WorldToCell(pickedUpOGPosition), pickedUpTileBase);

                        pickedUpTileComponent = null;
                        pickedUpTileBase = null;
                        return;
                    }
                    else
                    {
                        if (canBeMerged)
                        {

                            List<Vector3Int> tilesToBeMerged = EvaluateMergeTiles(placeLayer, placePosition.Value, GetTileComponent(placeLayer, placePosition.Value), true);

                            if (tilesToBeMerged == null)
                            {
                                // cancel merge and placement
                                pickedUpLayer.SetTile(pickedUpLayer.WorldToCell(pickedUpOGPosition), pickedUpTileBase);

                                pickedUpTileComponent = null;
                                pickedUpTileBase = null;
                                return;
                            }

                            if (MergeTiles(placeLayer, tilesToBeMerged, true) >= 0)
                            {
                                pickedUpTileComponent = null;
                                pickedUpTileBase = null;
                                return;
                            }
                            else
                            {
                                // cancel merge and placement
                                pickedUpLayer.SetTile(pickedUpLayer.WorldToCell(pickedUpOGPosition), pickedUpTileBase);

                                pickedUpTileComponent = null;
                                pickedUpTileBase = null;
                                return;
                            }

                            // do animation

                            // update here?

                        }

                        // update here?
                        placeLayer.SetTile(placePosition.Value, pickedUpTileBase);

                        pickedUpTileComponent = null;
                        pickedUpTileBase = null;
                        return;
                    }

                    //placementIndicator.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (pickedUpTileComponent != null)
            {
                pickedUpTileComponent.Release();
            }
            HighlightTile(null); // Remove highlight if not pointing at any tile
        }
    }

    public int SpawnTilesFromBlockRandomized(Vector3Int origin, Tilemap baseLayer, Tilemap spawnLayer, int maxAttempts, int maxTilesSpawn, int range = 1)
    {
        List<Vector3Int> candidates = new List<Vector3Int>();
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                candidates.Add(new Vector3Int(origin.x + dx, origin.y + dy, origin.z));
            }
        }
        // Shuffle the candidates.
        for (int i = 0; i < candidates.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, candidates.Count);
            Vector3Int temp = candidates[i];
            candidates[i] = candidates[rnd];
            candidates[rnd] = temp;
        }

        int spawned = 0;
        int attempts = 0;

        foreach (Vector3Int candidatePosition in candidates)
        {
            if (attempts >= maxAttempts || spawned >= maxTilesSpawn)
                break;

            attempts++;

            if (baseLayer.HasTile(candidatePosition) && !spawnLayer.HasTile(candidatePosition))
            {
                TileBlock tile = GetTileComponent(baseLayer, candidatePosition);
                try
                {
                    
                }
                catch(InvalidCastException e)
                {
                    continue;
                }

                SolidBlock block = tile as SolidBlock;
                if (block != null)
                {
                    TileBase spawnTile = block.SpawnTile();
                    if (spawnTile != null)
                    {
                        spawnLayer.SetTile(candidatePosition, spawnTile);
                        spawned++;
                    }
                }
            }
        }

        return spawned;
    }

    void TileHoverOrPickUp(RaycastHit raycastHit)
    {
        //aimedLayerIndex = GetLayerIndex(hit.point.y);
        aimedLayerIndex = GetLayerIndex(raycastHit.transform.parent.position.y + 0.4f);

        if (aimedLayerIndex >= 0 && aimedLayerIndex < worldLayers.Count)
        {
            //Debug.Log($"Pointing at layer: {aimedLayerIndex}");

            Tilemap targetLayer = worldLayers[aimedLayerIndex];
            Vector3Int tilePosition;

            if (aimedLayerIndex == 0)
            {
                tilePosition = targetLayer.WorldToCell(raycastHit.point);
            }
            else
            {
                Transform parent = raycastHit.transform.parent;
                Vector3Int anchor = targetLayer.WorldToCell(raycastHit.transform.parent.position);
                tilePosition = new (anchor.x, anchor.y, 0);
            }

            TileBlock tileComponent = GetTileComponent(targetLayer, tilePosition);

            HighlightTile(tileComponent);

            if (Input.GetMouseButtonDown(0) && tileComponent != null)
            {
                if (tileComponent.IsSolid & aimedLayerIndex < worldLayers.Count - 1)
                {
                    // solid blocks can not be picked up from underneeth any other tiles
                    if (GetTileComponent(worldLayers[aimedLayerIndex + 1], tilePosition) != null) { return; }
                }

                pickedUpTileComponent = tileComponent;

                if (pickedUpTileComponent.PickUp())
                {
                    pickedUpOGPosition = tileComponent.transform.position;
                    pickedUpTileBase = targetLayer.GetTile(tilePosition);
                    pickedUpLayerIndex = aimedLayerIndex;
                }
                else
                {
                    pickedUpTileComponent = null;
                    pickedUpTileBase = null;
                }
            }
        }
    }

    int GetLayerIndex(float yPosition)
    {
        for (int i = 0; i < worldLayers.Count; i++)
        {
            float layerY = worldLayers[i].transform.position.y;
            if (yPosition >= layerY + C1 && yPosition <= layerY + 0.5f + C2)
            {
                return i;
            }
        }
        return 0;
    }

    Tilemap GetLayer(Vector3 position, int yMod = 0)
    {
        int index = GetLayerIndex(position.y);
        if (index >= 0)
        {
            return worldLayers[index + yMod];
        }
        return null;
        
    }

    List<Vector3Int> EvaluateMergeTiles(Tilemap tilemap, Vector3Int origin, TileBlock startTile, bool additionalDrop)
    {
        var targetType = startTile.type;
        List<Vector3Int> mergeGroup = new List<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        queue.Enqueue(origin);
        visited.Add(origin);

        Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0)
        };

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            mergeGroup.Add(current);

            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;
                if (visited.Contains(neighbor))
                    continue;

                TileBlock neighborTile = GetTileComponent(tilemap, neighbor);
                if (neighborTile != null && neighborTile.type == targetType)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (additionalDrop)
        {
            return (mergeGroup.Count >= 2) ? mergeGroup : null;
        }
        else
        {
            return (mergeGroup.Count >= 3) ? mergeGroup : null;
        }       
    }

    // Merges the group of tiles by deleting them and placing new merged tiles.
    // For each group of three tiles, one new tile is placed. Leftover tiles are returned as an int.
    public static int MergeTiles(Tilemap tilemap, List<Vector3Int> mergeGroup, bool additionalDrop)
    {
        if (mergeGroup == null)
            return 0;

        int groups = (mergeGroup.Count + (additionalDrop ? 1 : 0)) / 3;
        int remainder = (mergeGroup.Count + (additionalDrop ? 1 : 0)) % 3;

        // Get the base tile from the origin (first position in the group)
        Vector3Int origin = mergeGroup[0];

        TileBase baseTile = tilemap.GetTile(origin);
        if (baseTile == null)
            return remainder;

        // Lookup the new tile from the GameManager merge progression dictionary.
        TileBase newTile;
        if (GameManager.Instance.mergeProgression.ContainsKey(baseTile))
            newTile = GameManager.Instance.mergeProgression[baseTile];
        else
            return -1;

        // Delete all tiles in the merge group.
        foreach (Vector3Int pos in mergeGroup)
        {
            tilemap.SetTile(pos, null);
        }

        // Place one new merged tile for every full group of three, using positions from mergeGroup.
        for (int i = 0; i < groups; i++)
        {
            Vector3Int placementPos = mergeGroup[i]; // Use positions from the list
            tilemap.SetTile(placementPos, newTile);
        }

        return remainder;
    }

    bool CanBreakTile(Tilemap tilemap, Vector3Int position, int layerIndex)
    {
        if (tilemap.GetTile(position) == null) { return false; } // No tile to break

        if (layerIndex + 1 < worldLayers.Count)
        {
            Tilemap aboveLayer = worldLayers[layerIndex + 1];
            if (aboveLayer.GetTile(position) != null) { return false; }
        }

        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (var dir in directions)
        {
            if (tilemap.GetTile(position + dir) == null) { return true; } // one of the neighbours is missin
        }
        
        return false;
    }

    public static TileBlock GetTileComponent(Tilemap tilemap, Vector3Int position)
    {
        GameObject tileObject = tilemap.GetInstantiatedObject(position);
        if (tileObject != null)
        {
            return tileObject.GetComponent<TileBlock>();
        }
        return null;
    }

    void HighlightTile(TileBlock tile)
    {
        if (lastHighlightedTile == tile) return; // Avoid redundant calls

        if (lastHighlightedTile != null)
        {
            lastHighlightedTile.Highlight(false);
        }

        if (tile != null)
        {
            tile.Highlight(true);
        }

        lastHighlightedTile = tile;
    }
}
