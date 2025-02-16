using UnityEngine;
using UnityEngine.Tilemaps;

public class ImmovableBlock : TileBlock
{
    public override (bool canBePlaced, bool canBeMerged, Tilemap level, Vector3Int? position) PlaceVerify(Vector3Int selectedTile, Vector3 hitNormal, Tilemap selectedLayer, Tilemap aboveLayer = null)
    {
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

    public override bool PickUp()
    {
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
