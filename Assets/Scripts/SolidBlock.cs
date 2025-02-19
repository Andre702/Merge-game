using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SolidBlock : TileBlock
{
    [System.Serializable] public class TileChance
    {
        public TileBase tile;
        public float chanceMod;
    }

    public List<TileChance> possibleTileSpawns;

    public override bool IsSolid => true;

    public override (bool canBePlaced, bool canBeMerged, Tilemap level, Vector3Int? position) PlaceVerify(Vector3Int selectedTile, Vector3 hitNormal, Tilemap selectedLayer, Tilemap aboveLayer = null)
    {
        Vector3Int? placePosition = null;
        if (Vector3.Dot(hitNormal, Vector3.up) > 0.95f) // TOP ----------------------------------
        {
            if (aboveLayer == null) { return (false, false, null, null); } // if build limit reached

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
                return (true, false, selectedLayer, placePosition);
            }
        }

        // else: an unidentified side was hit (somehow back or bottom) or the tile is already occupied
        return (false, false, null, null);
    }

    public override (bool canBePlaced, bool canBeMerged, Tilemap level, Vector3Int? position) PlaceVerify(Vector3Int selectedTile, Vector3 hitNormal, Tilemap aboveLayer)
    {
        if (Vector3.Dot(hitNormal, Vector3.up) > 0.95f) // TOP ----------------------------------
        {
            if (aboveLayer == null) { return (false, false, null, null); } // if build limit reached

            if (aboveLayer.GetTile(selectedTile) != null) { return (false, false, null, null); } // if there is SOMEHOW no space above

            return (true, false, aboveLayer, selectedTile);
        }
        return (false, false, null, null);
    }

    

    public TileBase SpawnTile()
    {
        if (possibleTileSpawns == null || possibleTileSpawns.Count == 0)
            return null; // No possible tiles to spawn.

        // Calculate total weight.
        float totalWeight = possibleTileSpawns.Sum(entry => entry.chanceMod);

        if (totalWeight <= 0)
        {
            Debug.LogWarning("possibleTileSpawns has members to spawn but their total weight is equal 0!");
            return null; // No valid spawn chances.
        }
            
        float randomValue = Random.Range(0, totalWeight);

        // Pick the tile based on weighted probabilities.
        float cumulativeWeight = 0;
        foreach (var entry in possibleTileSpawns)
        {
            cumulativeWeight += entry.chanceMod;
            if (randomValue <= cumulativeWeight)
                return entry.tile;
        }

        return null; // Fallback (should never reach here)
    }

    public override bool UpdateTile()
    {
        throw new System.NotImplementedException();
    }

}
