using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class PlatformerDecorationCollisionGuard
{
    private static readonly HashSet<Tile> FixedTiles = new HashSet<Tile>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void DisableDecorationTileCollisions()
    {
        foreach (Tilemap tilemap in Object.FindObjectsByType<Tilemap>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            using var usedTiles = tilemap.GetUsedTiles(Allocator.Temp);
            bool changed = false;

            for (int i = 0; i < usedTiles.Length; i++)
            {
                if (usedTiles[i] is not Tile tile)
                {
                    continue;
                }

                if (!tile.name.StartsWith("PlatformerTile_") ||
                    tile.colliderType == Tile.ColliderType.None)
                {
                    continue;
                }

                // Shared Tile assets only need fixing once across all scenes.
                if (!FixedTiles.Add(tile))
                {
                    continue;
                }

                tile.colliderType = Tile.ColliderType.None;
                changed = true;
            }

            if (changed)
            {
                tilemap.RefreshAllTiles();
            }
        }
    }
}
