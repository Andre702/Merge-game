using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ItemType
{
    None,
    Grass,
    Stick,
    Log,
    Stone,
    BrownCoal,
    Copper,
    Mushroom,
    Silk
}

public class NonSolidBlock : TileBlock
{
    [System.Serializable]
    public class ItemChance
    {
        public ItemType item;
        public int amount;
        public float chanceWeight;
    }

    public List<ItemChance> possibleItemDrops;

    public override bool IsSolid => false;

    public override (bool canBePlaced, bool canBeMerged, Tilemap level, Vector3Int? position) PlaceVerify(Vector3Int selectedTile, Vector3 hitNormal, Tilemap selectedLayer, Tilemap aboveLayer = null)
    {
        TileBlock target = World.GetTileComponent(selectedLayer, selectedTile);
        if (!target.IsSolid & target.type == this.type)
        {
            return (true, true, selectedLayer, selectedTile);
        }

        Vector3Int? placePosition = null;
        if (Vector3.Dot(hitNormal, Vector3.up) > 0.95f) // TOP ----------------------------------
        {
            if (aboveLayer == null) { return (false, false, null, null); } // if the build limit reached

            if (!target.IsSolid | !target.canPlaceOnTop) { return (false, false, null, null); } // if no solid ground beneeth

            if (aboveLayer.GetTile(selectedTile) != null) { return (false, false, null, null); } // if there is SOMEHOW no space above

            return (true, false, aboveLayer, selectedTile);
        }

        else if (Vector3.Dot(hitNormal, Vector3.left) > 0.95f) // LEFT SIDE ------------------------
        {
            placePosition = selectedTile + Vector3Int.left;
        }

        else if (Vector3.Dot(hitNormal, Vector3.back) > 0.95f) // RIGHT SIDE -----------------------
        {
            placePosition = selectedTile + Vector3Int.down;
        }

        if (placePosition.HasValue) // If either left or right side was hit
        {
            if (selectedLayer.GetTile(placePosition.Value) == null) // and there is no other tile in this direction
            {
                Tilemap floor = OnFloorSpace(placePosition.Value, selectedLayer); // check for solid floor
                if (floor != null)
                {
                    if (floor.GetTile(placePosition.Value) != null /*& can be merged with the tile held in cursor*/) // (if there is a tile on a floor it must not be solid this was already confirmed in OnFloorSpace)
                    {
                        return (false, false, null, null);
                        // the floor is not empty and the tile can be merged with the one held in cursor
                        // merge
                        //return (true, true, floor, placePosition);
                    }

                    // there is an empty space on solid floor (floor.GetTile(placePosition.Value) == null)
                    return (true, false, floor, placePosition);
                }
            }
        }

        // else: an unidentified side was hit (somehow back or bottom) or the tile is already occupied
        return (false, false, null, null);
    }

    public override (bool canBePlaced, bool canBeMerged, Tilemap level, Vector3Int? position) PlaceVerify(Vector3Int selectedTile, Vector3 hitNormal, Tilemap aboveLayer)
    {
        return (false, false, null, null);
    }

    public (ItemType item, int amount) GetItems(float amountMod, float chanceMod)
    {
        if (possibleItemDrops == null || possibleItemDrops.Count < 2)
        {
            Debug.LogWarning("No valid items configured for this block.");
            return (default, 0);
        }

        // First item (index 0) is always "nothing"
        float nothingWeight = possibleItemDrops[0].chanceWeight * (1 - chanceMod);

        // Calculate total weight (including "nothing")
        float totalWeight = possibleItemDrops.Sum(entry => entry.chanceWeight);

        if (totalWeight <= 0)
        {
            Debug.LogWarning("possibleItemDrops has members, but total weight is 0!");
            return (default, 0);
        }

        // Random number for selection
        float randomValue = Random.Range(0, totalWeight);

        // Pick an item or return nothing
        float cumulativeWeight = nothingWeight;
        if (randomValue <= cumulativeWeight)
            return (default, 0); // No item drop

        for (int i = 1; i < possibleItemDrops.Count; i++) // Skip index 0 (nothing)
        {
            cumulativeWeight += possibleItemDrops[i].chanceWeight;
            if (randomValue <= cumulativeWeight)
            {
                return (possibleItemDrops[i].item, Mathf.RoundToInt(possibleItemDrops[i].amount * 1 + amountMod));
            }
        }

        return (default, 0); // Fallback, shouldn't reach
    }

    public override bool UpdateTile()
    {
        throw new System.NotImplementedException();
    }

    Vector3? SnapTileToFloorRaycast(Vector3 origin)
    {
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitFloor))
        {
            return hitFloor.point;
        }

        return null;
    }

    Tilemap OnFloorSpace(Vector3Int position, Tilemap selectedLayer)
    {
        int i = GameManager.Instance.currentWorld.worldLayers.IndexOf(selectedLayer);

        i--;

        while (i >= 0)
        {
            Tilemap layer = GameManager.Instance.currentWorld.worldLayers[i];
            if (layer.GetTile(position) != null)
            {
                if(World.GetTileComponent(layer, position).IsSolid)
                {
                    return GameManager.Instance.currentWorld.worldLayers[i+1];
                }

                return layer;
            }

            i--;
        }

        return null;
    }

    
}
