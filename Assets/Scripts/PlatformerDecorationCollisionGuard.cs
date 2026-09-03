using UnityEngine;
using UnityEngine.Tilemaps;

public static class PlatformerDecorationCollisionGuard
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void DisableDecorationTileCollisions()
    {
        foreach (Tilemap tilemap in Object.FindObjectsByType<Tilemap>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            bool changed = false;

            foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
            {
                TileBase tileBase = tilemap.GetTile(cell);
                if (tileBase is Tile tile &&
                    tile.name.StartsWith("PlatformerTile_") &&
                    tile.colliderType != Tile.ColliderType.None)
                {
                    tile.colliderType = Tile.ColliderType.None;
                    changed = true;
                }
            }

            if (changed)
            {
                tilemap.RefreshAllTiles();
            }
        }
    }
}
