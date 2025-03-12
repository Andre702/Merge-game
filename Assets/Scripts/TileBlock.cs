using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    UNLABELED,

    GrassBlock,
    DirtBlock,
    HeyRoofL,

    ShortGrass,
    MediumGrass,
    TallGrass,
    SmallBush,
    MediumBush,
    BigBush,
    SmallTree,
    MediumTree,
    BigTree,
    SmallStone,
    MediumStone,
    BigStone,
    SmallBrownCoalOre,
    MediumBrownCoalOre,
    BigBrownCoalOre,
    SmallCopperlOre,
    MediumCopperlOre,
    BigCopperlOre,
}

public abstract class TileBlock : MonoBehaviour
{
    public SpriteRenderer sprite;
    public virtual bool IsSolid { get; } = true;

    private bool isPickedUp = false;
    private float pickupHeight = 1f;
    public bool canPlaceOnTop = true;


    public TileType type;

    public void Highlight(bool highlight)
    {
        if (highlight == true)
        {
            sprite.color = new Color(0.92f, 0.92f, 0.92f, 1f);
        }
        else
        {
            sprite.color = Color.white;
        }
    }

    protected void Update()
    {
        if (isPickedUp)
        {
            FollowMouse();
        }
    }

    public virtual bool PickUp()
    {
        gameObject.layer = 2; // Layer: Ignore Raycast
        isPickedUp = true;
        return true;
    }

    public virtual void Release()
    {
        isPickedUp = false;
    }

    public void ResetVisibility()
    {
        gameObject.layer = 0; // Layer: Default
    }


    private void FollowMouse()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            transform.position = hit.point + Vector3.up * pickupHeight + new Vector3(0, 0, 0.2f);
        }
    }

    public abstract (bool canBePlaced, bool canBeMerged, Tilemap level, Vector3Int? position) PlaceVerify(Vector3Int selectedTile, Vector3 hitNormal, Tilemap selectedLayer, Tilemap aboveLayer = null);
    public abstract (bool canBePlaced, bool canBeMerged, Tilemap level, Vector3Int? position) PlaceVerify(Vector3Int selectedTile, Vector3 hitNormal, Tilemap aboveLayer);

    public abstract bool UpdateTile();

    public void Placed()
    {
        // update blocks around
    }

}
