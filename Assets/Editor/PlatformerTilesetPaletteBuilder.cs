using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class PlatformerTilesetPaletteBuilder
{
    private const string SpriteSheetPath =
        "Assets/PlatformerTileset/TileSet/SprTiles.png";
    private const string GeneratedFolder =
        "Assets/PlatformerTileset/GeneratedTiles";
    private const string PalettePath =
        "Assets/PlatformerTileset/PlatformerTilesetPalette.prefab";
    private const string PaletteTemplatePath = "Assets/SpikePalette.prefab";
    private const int PaletteColumns = 8;
    private const float CorrectPixelsPerUnit = 16f;
    private const float SpriteScaleToOneCell = 1f;

    private static int remainingDelayFrames = 5;

    static PlatformerTilesetPaletteBuilder()
    {
        EditorApplication.update -= BuildWhenEditorIsReady;
        EditorApplication.update += BuildWhenEditorIsReady;
    }

    [MenuItem("Assets/Rebuild Platformer Tileset Palette")]
    public static void RebuildFromMenu()
    {
        BuildPalette();
    }

    private static void BuildWhenEditorIsReady()
    {
        if (EditorApplication.isCompiling ||
            EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (remainingDelayFrames-- > 0)
        {
            return;
        }

        EditorApplication.update -= BuildWhenEditorIsReady;

        Tile firstTile = AssetDatabase.LoadAssetAtPath<Tile>(
            $"{GeneratedFolder}/PlatformerTile_00.asset");
        bool needsSizeUpgrade =
            firstTile == null ||
            !Mathf.Approximately(firstTile.transform.m00, SpriteScaleToOneCell) ||
            firstTile.colliderType != Tile.ColliderType.None;

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) == null ||
            needsSizeUpgrade)
        {
            BuildPalette();
        }
    }

    private static void BuildPalette()
    {
        ConfigureSpriteSheetSize();

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(SpriteSheetPath)
            .OfType<Sprite>()
            .OrderByDescending(sprite => sprite.rect.y)
            .ThenBy(sprite => sprite.rect.x)
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError(
                $"[PlatformerTilesetPaletteBuilder] Sprite bulunamadi: {SpriteSheetPath}");
            return;
        }

        EnsureGeneratedFolder();
        List<Tile> tiles = CreateOrUpdateTiles(sprites);

        if (!EnsurePaletteAsset())
        {
            return;
        }

        PopulatePalette(tiles);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[PlatformerTilesetPaletteBuilder] {tiles.Count} tile ile palette hazir: {PalettePath}");
    }

    private static void ConfigureSpriteSheetSize()
    {
        TextureImporter importer =
            AssetImporter.GetAtPath(SpriteSheetPath) as TextureImporter;
        if (importer == null ||
            Mathf.Approximately(importer.spritePixelsPerUnit, CorrectPixelsPerUnit))
        {
            return;
        }

        importer.spritePixelsPerUnit = CorrectPixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static List<Tile> CreateOrUpdateTiles(Sprite[] sprites)
    {
        var tiles = new List<Tile>(sprites.Length);

        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                string tilePath = $"{GeneratedFolder}/PlatformerTile_{i:D2}.asset";
                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);

                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    AssetDatabase.CreateAsset(tile, tilePath);
                }

                tile.name = $"PlatformerTile_{i:D2}";
                tile.sprite = sprites[i];
                tile.color = Color.white;
                Vector3 scale = new Vector3(
                    SpriteScaleToOneCell,
                    SpriteScaleToOneCell,
                    1f);
                Vector3 pivotCorrection = -Vector3.Scale(
                    sprites[i].bounds.center,
                    scale);
                tile.transform = Matrix4x4.TRS(
                    pivotCorrection,
                    Quaternion.identity,
                    scale);
                tile.colliderType = Tile.ColliderType.None;
                EditorUtility.SetDirty(tile);
                tiles.Add(tile);
            }

            AssetDatabase.SaveAssets();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        return tiles;
    }

    private static bool EnsurePaletteAsset()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) != null)
        {
            return true;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(PaletteTemplatePath) == null)
        {
            Debug.LogError(
                $"[PlatformerTilesetPaletteBuilder] Palette sablonu bulunamadi: {PaletteTemplatePath}");
            return false;
        }

        if (!AssetDatabase.CopyAsset(PaletteTemplatePath, PalettePath))
        {
            Debug.LogError(
                $"[PlatformerTilesetPaletteBuilder] Palette olusturulamadi: {PalettePath}");
            return false;
        }

        AssetDatabase.ImportAsset(PalettePath);
        return true;
    }

    private static void PopulatePalette(IReadOnlyList<Tile> tiles)
    {
        GameObject paletteRoot = PrefabUtility.LoadPrefabContents(PalettePath);
        try
        {
            paletteRoot.name = "PlatformerTilesetPalette";

            Grid grid = paletteRoot.GetComponent<Grid>();
            if (grid != null)
            {
                grid.cellSize = Vector3.one;
            }

            Tilemap tilemap = paletteRoot.GetComponentInChildren<Tilemap>(true);
            if (tilemap == null)
            {
                var layer = new GameObject("Platformer Tiles");
                layer.transform.SetParent(paletteRoot.transform, false);
                tilemap = layer.AddComponent<Tilemap>();
                layer.AddComponent<TilemapRenderer>();
            }

            tilemap.gameObject.name = "Platformer Tiles";
            tilemap.ClearAllTiles();

            for (int i = 0; i < tiles.Count; i++)
            {
                int x = i % PaletteColumns;
                int y = -(i / PaletteColumns);
                tilemap.SetTile(new Vector3Int(x, y, 0), tiles[i]);
            }

            tilemap.CompressBounds();
            PrefabUtility.SaveAsPrefabAsset(paletteRoot, PalettePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(paletteRoot);
        }
    }

    private static void EnsureGeneratedFolder()
    {
        const string root = "Assets/PlatformerTileset";
        if (!AssetDatabase.IsValidFolder(GeneratedFolder))
        {
            AssetDatabase.CreateFolder(root, "GeneratedTiles");
        }
    }
}
