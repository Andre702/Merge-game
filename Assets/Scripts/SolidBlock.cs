using UnityEngine;
using UnityEngine.Tilemaps;

public class SolidBlock : TileBlock
{
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

    public override bool UpdateTile()
    {
        throw new System.NotImplementedException();
    }

}
