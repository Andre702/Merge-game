using UnityEngine;
using UnityEngine.Tilemaps;

public class NonSolidBlock : TileBlock
{
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

            if (!World.GetTileComponent(selectedLayer, selectedTile).IsSolid) { return (false, false, null, null); } // if no solid ground beneeth

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
