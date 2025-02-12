using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class World : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] List<Tilemap> worldLayers;

    private const float C1 = 0.02f;
    private const float C2 = 0.001f;

    private TileBlock lastHighlightedTile = null; // Stores the last highlighted tile

    void Update()
    {
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            Vector3 hitPosition = hit.point;
            int layerIndex = GetLayerIndex(hitPosition.y);

            if (layerIndex >= 0 && layerIndex < worldLayers.Count)
            {
                Debug.Log($"Pointing at layer: {layerIndex}");

                Tilemap targetLayer = worldLayers[layerIndex];
                Vector3Int tilePosition = targetLayer.WorldToCell(hitPosition);

                TileBlock tileComponent = GetTileComponent(targetLayer, tilePosition);
                HighlightTile(tileComponent);

                if (Input.GetMouseButtonDown(0)) // Left-click to break tile
                {
                    if (CanBreakTile(targetLayer, tilePosition, layerIndex))
                    {
                        targetLayer.SetTile(tilePosition, null);
                        HighlightTile(null); // Remove highlight after breaking
                    }
                }
            }
        }
        else
        {
            HighlightTile(null); // Remove highlight if not pointing at any tile
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
        return -1; // Not in any layer
    }

    bool CanBreakTile(Tilemap tilemap, Vector3Int position, int layerIndex)
    {
        if (tilemap.GetTile(position) == null)
            return false; // No tile to break

        int neighbors = 0;
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (var dir in directions)
        {
            if (tilemap.GetTile(position + dir) != null)
                neighbors++;
        }

        if (neighbors == 4)
            return false; // Fully surrounded tile cannot be broken

        if (layerIndex + 1 < worldLayers.Count)
        {
            Tilemap aboveLayer = worldLayers[layerIndex + 1];
            if (aboveLayer.GetTile(position) != null)
                return false; // Tile is blocked by one above
        }

        return true; // Tile can be broken
    }

    TileBlock GetTileComponent(Tilemap tilemap, Vector3Int position)
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
