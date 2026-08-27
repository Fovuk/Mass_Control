using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapLevelAnalyzer
{
    [MenuItem("Tools/Analyze Active Level Tilemap")]
    public static void AnalyzeActiveLevel()
    {
        var tilemaps = Object.FindObjectsByType<Tilemap>();
        if (tilemaps.Length == 0)
        {
            Debug.LogWarning("[TilemapLevelAnalyzer] No tilemaps found in active scene.");
            return;
        }

        var report = new StringBuilder();
        report.AppendLine("[TilemapLevelAnalyzer] Report");

        foreach (var tilemap in tilemaps)
        {
            tilemap.CompressBounds();
            var bounds = tilemap.cellBounds;
            var count = 0;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                    count++;
            }

            report.AppendLine($"- {tilemap.name}: cells={count}, bounds={bounds.size}, origin={bounds.min}");
        }

        Debug.Log(report.ToString());
    }
}
