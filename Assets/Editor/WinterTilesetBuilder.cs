using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Winter 128x128 sprites must use PPU 128 so each tile fits a 1x1 grid cell
/// (100 PPU on 128px art overflows into neighboring cells).
/// </summary>
public static class WinterTilesetBuilder
{
    const string WinterFolder =
        "Assets/Free Platform Game Assets/Update 1.9/New Tiles (2D view)/Winter/128x128";

    const int TilePixelSize = 128;
    const float TilePixelsPerUnit = 128f;

    [MenuItem("Tools/Fix Winter 128x128 Tileset")]
    public static void FixWinterTileset()
    {
        var pngPaths = AssetDatabase.FindAssets("t:Texture2D", new[] { WinterFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p)
            .ToArray();

        int fixedTextures = 0;
        int createdTiles = 0;

        foreach (var pngPath in pngPaths)
        {
            if (FixTextureImport(pngPath))
            {
                fixedTextures++;
            }

            if (CreateTileAssetIfMissing(pngPath))
            {
                createdTiles++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(
            $"[WinterTilesetBuilder] Fixed {fixedTextures} textures, created {createdTiles} tile assets in {WinterFolder}");
    }

    static bool FixTextureImport(string pngPath)
    {
        var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        if (importer == null)
        {
            return false;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = TilePixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.alphaIsTransparency = true;

        var sheet = importer.spritesheet;
        if (sheet == null || sheet.Length == 0)
        {
            var baseName = Path.GetFileNameWithoutExtension(pngPath);
            sheet = new[]
            {
                new SpriteMetaData
                {
                    name = baseName + "_0",
                    rect = new Rect(0, 0, TilePixelSize, TilePixelSize),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    border = Vector4.zero
                }
            };
        }
        else
        {
            for (int i = 0; i < sheet.Length; i++)
            {
                sheet[i].rect = new Rect(0, 0, TilePixelSize, TilePixelSize);
                sheet[i].alignment = (int)SpriteAlignment.Center;
                sheet[i].pivot = new Vector2(0.5f, 0.5f);
            }
        }

        importer.spritesheet = sheet;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteExtrude = 1;
        importer.SetTextureSettings(settings);

        importer.SaveAndReimport();
        return true;
    }

    static bool CreateTileAssetIfMissing(string pngPath)
    {
        var dir = Path.GetDirectoryName(pngPath).Replace('\\', '/');
        var sprites = AssetDatabase.LoadAllAssetsAtPath(pngPath).OfType<Sprite>().ToArray();
        if (sprites.Length == 0)
        {
            Debug.LogWarning($"[WinterTilesetBuilder] No sprites at {pngPath}");
            return false;
        }

        bool anyCreated = false;
        foreach (var sprite in sprites)
        {
            var tilePath = $"{dir}/{sprite.name}.asset";
            if (AssetDatabase.LoadAssetAtPath<Tile>(tilePath) != null)
            {
                continue;
            }

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = sprite.name;
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;

            AssetDatabase.CreateAsset(tile, tilePath);
            anyCreated = true;
        }

        return anyCreated;
    }
}
