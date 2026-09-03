using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class EnvironmentPropsBuilder
{
    private const int BuildVersion = 4;
    private const string Level6SpriteFolder =
        "Assets/Resources/EnvironmentProps/Level6";
    private const string GeneratedTileFolder =
        "Assets/PlatformerTileset/GeneratedProps";
    private const string LibraryPath =
        "Assets/Resources/EnvironmentPropLibrary.asset";
    private const string PalettePath =
        "Assets/PlatformerTileset/EnvironmentPropsPalette.prefab";
    private const string PaletteTemplatePath = "Assets/SpikePalette.prefab";

    private static readonly PropSource[] Sources =
    {
        new PropSource("SpringTree", "Assets/Free Platform Game Assets/Update 1.9/New Details/2x/Tree.png", "Tree_0", 0.60f),
        new PropSource("AutumnTree", "Assets/Free Platform Game Assets/Update 1.9/New Details/2x/Autumn Tree.png", "Autumn Tree_0", 0.60f),
        new PropSource("Bush", "Assets/Free Platform Game Assets/Update 1.9/New Details/2x/Bush.png", "Bush_0", 1f),
        new PropSource("AutumnBush", "Assets/Free Platform Game Assets/Update 1.9/New Details/2x/Autumn Bush.png", "Autumn Bush_0", 1f),
        new PropSource("SmallTree", "Assets/Free Platform Game Assets/Platform Game Assets/Environment/png/All/1x/Asset 1.png", "Asset 1_0", 1f),
        new PropSource("BlockTree", "Assets/Free Platform Game Assets/Platform Game Assets/Environment/png/All/1x/Asset 3.png", "Asset 3_0", 1f),
        new PropSource("SmallBush", "Assets/Free Platform Game Assets/Platform Game Assets/Environment/png/All/1x/Asset 4.png", "Asset 4_0", 1.4f),
        new PropSource("RockGrass", "Assets/Free Platform Game Assets/Platform Game Assets/Environment/png/All/1x/Asset 8.png", "Asset 8_0", 2f),
        new PropSource("RockGrassAlt", "Assets/Free Platform Game Assets/Platform Game Assets/Environment/png/All/1x/Asset 10.png", "Asset 10_0", 2f),
        new PropSource("YellowFlower", "Assets/Free Platform Game Assets/Platform Game Assets/Environment/png/All/1x/Asset 14.png", "Asset 14_0", 1.8f),
        new PropSource("GrassFlower", "Assets/Free Platform Game Assets/Platform Game Assets/Environment/png/All/1x/Asset 15.png", "Asset 15_0", 1.6f),
        new PropSource("Flower1", "Assets/Free Platform Game Assets/Update 1.9/New Details/2x/Flower-1.png", "Flower-1_0", 0.9f),
        new PropSource("Flower2", "Assets/Free Platform Game Assets/Update 1.9/New Details/2x/Flower-2.png", "Flower-2_0", 0.9f),
        new PropSource("TreasureChest", "Assets/Free Platform Game Assets/Platform Game Assets/Chest/png/Separate/1x/1-1x.png", "1-1x_0", 1f),
        new PropSource("StoneColumn", "Assets/Free Platform Game Assets/Update 1.8.5/New Tiles/png/128x128/column_down.png", "column_down_0", 1f),
        new PropSource("StreetLamp", "Assets/Resources/EnvironmentProps/StreetLamp.png", "StreetLamp_0", 1f),
        new PropSource("StreetLampTall", "Assets/Resources/EnvironmentProps/StreetLamp.png", "StreetLamp_0", 1.2f),
        new PropSource("StreetLampSmall", "Assets/Resources/EnvironmentProps/StreetLamp.png", "StreetLamp_0", 0.85f),
        new PropSource("WoodPile", "Assets/Resources/EnvironmentProps/WoodPile.png", "WoodPile_0", 1f),
        new PropSource("WoodPileWide", "Assets/Resources/EnvironmentProps/WoodPile.png", "WoodPile_0", 1.25f),
        new PropSource("WoodenCabin", "Assets/PlatformerTileset/Objects/ObjHouse.png", "ObjHouse", 4f),
        new PropSource("WinterTree", "Assets/Free Platform Game Assets/Update 1.9/New Details/2x/Winter Tree.png", "Winter Tree_0", 0.6f),
        new PropSource("SnowCap", "Assets/Free Platform Game Assets/Update 1.9/New Tiles (2D view)/Winter/128x128/GrassMid.png", "GrassMid_0", 1f),
        new PropSource("SnowColumn", "Assets/Free Platform Game Assets/Update 1.9/New Tiles (2D view)/Winter/128x128/GrassColumn.png", "GrassColumn_0", 1f),
        new PropSource("BareRock", Level6SpriteFolder + "/BareRock.png", "BareRock", 1f),
        new PropSource("BareRockPile", Level6SpriteFolder + "/BareRockPile.png", "BareRockPile", 1f),
        new PropSource("HangingChain", Level6SpriteFolder + "/HangingChain.png", "HangingChain", 1f),
        new PropSource("Skull", Level6SpriteFolder + "/Skull.png", "Skull", 1f),
        new PropSource("CrossedBones", Level6SpriteFolder + "/CrossedBones.png", "CrossedBones", 1f),
        new PropSource("GroundPickaxe", Level6SpriteFolder + "/GroundPickaxe.png", "GroundPickaxe", 1f),
        new PropSource("GroundShovel", Level6SpriteFolder + "/GroundShovel.png", "GroundShovel", 1f),
        new PropSource("GroundHammer", Level6SpriteFolder + "/GroundHammer.png", "GroundHammer", 1f),
    };

    private static int delayFrames = 8;

    static EnvironmentPropsBuilder()
    {
        EditorApplication.update -= BuildWhenReady;
        EditorApplication.update += BuildWhenReady;
    }

    [MenuItem("Assets/Rebuild Environment Props Palette")]
    public static void Build()
    {
        EnsureFolders();
        EnsureLevel6Sprites();
        List<LoadedProp> props = LoadProps();
        if (props.Count == 0)
        {
            Debug.LogError("[EnvironmentPropsBuilder] Free Platform Game Assets icinde prop bulunamadi.");
            return;
        }

        EnvironmentPropLibrary library = CreateOrUpdateLibrary(props);
        List<Tile> tiles = CreateOrUpdateTiles(props);
        CreateOrUpdatePalette(tiles);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[EnvironmentPropsBuilder] {tiles.Count} dokunulmaz prop palette'e eklendi.");
    }

    private static void BuildWhenReady()
    {
        if (EditorApplication.isCompiling ||
            EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (delayFrames-- > 0)
        {
            return;
        }

        EditorApplication.update -= BuildWhenReady;
        EnvironmentPropLibrary library =
            AssetDatabase.LoadAssetAtPath<EnvironmentPropLibrary>(LibraryPath);
        if (library == null || library.buildVersion < BuildVersion)
        {
            Build();
        }
    }

    private static List<LoadedProp> LoadProps()
    {
        var loaded = new List<LoadedProp>();
        foreach (PropSource source in Sources)
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(source.path)
                .OfType<Sprite>()
                .ToArray();
            Sprite sprite = sprites.FirstOrDefault(
                item => item.name == source.spriteName);
            if (sprite == null && sprites.Length == 1)
            {
                sprite = sprites[0];
            }
            if (sprite != null)
            {
                loaded.Add(new LoadedProp(source, sprite));
            }
            else
            {
                Debug.LogWarning($"[EnvironmentPropsBuilder] Sprite bulunamadi: {source.path}");
            }
        }

        return loaded;
    }

    private static EnvironmentPropLibrary CreateOrUpdateLibrary(
        IReadOnlyList<LoadedProp> props)
    {
        EnvironmentPropLibrary library =
            AssetDatabase.LoadAssetAtPath<EnvironmentPropLibrary>(LibraryPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<EnvironmentPropLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
        }

        Sprite[] all = props.Select(prop => prop.sprite).ToArray();
        library.buildVersion = BuildVersion;
        library.allProps = all;
        library.meadowProps = Select(
            props,
            "SpringTree", "Bush", "SmallTree", "SmallBush",
            "RockGrass", "YellowFlower", "GrassFlower", "Flower1");
        library.canyonProps = Select(
            props,
            "RockGrass", "StoneColumn", "AutumnBush", "RockGrassAlt",
            "TreasureChest", "BlockTree", "Flower2");
        library.settlementProps = Select(
            props,
            "StoneColumn", "TreasureChest", "AutumnTree", "AutumnBush",
            "RockGrassAlt", "SmallBush", "GrassFlower", "StreetLamp",
            "StreetLampTall", "WoodPile", "WoodenCabin");
        library.winterProps = Select(
            props,
            "WinterTree", "SnowCap", "SnowColumn", "StreetLampSmall",
            "WoodPile", "RockGrassAlt", "TreasureChest");
        library.level6Props = Select(
            props,
            "BareRock", "HangingChain", "Skull", "GroundPickaxe",
            "BareRockPile", "CrossedBones", "GroundShovel", "GroundHammer");
        EditorUtility.SetDirty(library);
        return library;
    }

    private static Sprite[] Select(
        IReadOnlyList<LoadedProp> props,
        params string[] names)
    {
        return names
            .Select(name => props.FirstOrDefault(prop => prop.source.name == name).sprite)
            .Where(sprite => sprite != null)
            .ToArray();
    }

    private static List<Tile> CreateOrUpdateTiles(IReadOnlyList<LoadedProp> props)
    {
        var tiles = new List<Tile>(props.Count);
        foreach (LoadedProp prop in props)
        {
            string path = $"{GeneratedTileFolder}/{prop.source.name}Tile.asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            Vector3 scale = new Vector3(
                prop.source.scale,
                prop.source.scale,
                1f);
            Bounds bounds = prop.sprite.bounds;
            Vector3 bottomAlignment = new Vector3(
                -bounds.center.x * scale.x,
                -0.5f - bounds.min.y * scale.y,
                0f);

            tile.name = prop.source.name + "Tile";
            tile.sprite = prop.sprite;
            tile.color = Color.white;
            tile.transform = Matrix4x4.TRS(
                bottomAlignment,
                Quaternion.identity,
                scale);
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            tiles.Add(tile);
        }

        return tiles;
    }

    private static void CreateOrUpdatePalette(IReadOnlyList<Tile> tiles)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) == null)
        {
            if (!AssetDatabase.CopyAsset(PaletteTemplatePath, PalettePath))
            {
                Debug.LogError("[EnvironmentPropsBuilder] Palette olusturulamadi.");
                return;
            }
            AssetDatabase.ImportAsset(PalettePath);
        }

        GameObject root = PrefabUtility.LoadPrefabContents(PalettePath);
        try
        {
            root.name = "EnvironmentPropsPalette";
            Grid grid = root.GetComponent<Grid>();
            if (grid != null)
            {
                grid.cellSize = Vector3.one;
            }

            Tilemap tilemap = root.GetComponentInChildren<Tilemap>(true);
            tilemap.gameObject.name = "Free Platform Props";

            var existingTiles = new HashSet<TileBase>();
            foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
            {
                TileBase existing = tilemap.GetTile(cell);
                if (existing != null)
                {
                    existingTiles.Add(existing);
                }
            }

            int nextSlot = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                if (existingTiles.Contains(tiles[i]))
                {
                    continue;
                }

                Vector3Int position;
                do
                {
                    int x = (nextSlot % 5) * 5;
                    int y = -(nextSlot / 5) * 6;
                    position = new Vector3Int(x, y, 0);
                    nextSlot++;
                }
                while (tilemap.HasTile(position));

                tilemap.SetTile(position, tiles[i]);
                existingTiles.Add(tiles[i]);
            }

            tilemap.CompressBounds();
            PrefabUtility.SaveAsPrefabAsset(root, PalettePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        if (!AssetDatabase.IsValidFolder(GeneratedTileFolder))
        {
            AssetDatabase.CreateFolder(
                "Assets/PlatformerTileset",
                "GeneratedProps");
        }
        Directory.CreateDirectory(Level6SpriteFolder);
    }

    private static void EnsureLevel6Sprites()
    {
        CreatePixelProp("BareRock", DrawBareRock);
        CreatePixelProp("BareRockPile", DrawBareRockPile);
        CreatePixelProp("HangingChain", DrawHangingChain);
        CreatePixelProp("Skull", DrawSkull);
        CreatePixelProp("CrossedBones", DrawCrossedBones);
        CreatePixelProp("GroundPickaxe", DrawPickaxe);
        CreatePixelProp("GroundShovel", DrawShovel);
        CreatePixelProp("GroundHammer", DrawHammer);
    }

    private static void CreatePixelProp(
        string name,
        System.Action<Color32[]> draw)
    {
        string path = $"{Level6SpriteFolder}/{name}.png";
        if (!File.Exists(path))
        {
            var pixels = new Color32[32 * 32];
            draw(pixels);
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private static void DrawBareRock(Color32[] pixels)
    {
        Color32 outline = Hex("#292B35");
        Color32 dark = Hex("#4A4C58");
        Color32 mid = Hex("#686B77");
        Color32 light = Hex("#8A8D97");
        FillRect(pixels, 5, 3, 22, 3, outline);
        FillRect(pixels, 7, 6, 18, 3, dark);
        FillRect(pixels, 9, 9, 14, 4, mid);
        FillRect(pixels, 12, 13, 8, 2, outline);
        FillRect(pixels, 11, 10, 6, 2, light);
        FillRect(pixels, 7, 7, 3, 2, mid);
        Set(pixels, 6, 6, outline);
        Set(pixels, 25, 6, outline);
    }

    private static void DrawBareRockPile(Color32[] pixels)
    {
        Color32 outline = Hex("#292B35");
        Color32 dark = Hex("#484B55");
        Color32 mid = Hex("#6E707B");
        FillEllipse(pixels, 10, 7, 7, 5, outline);
        FillEllipse(pixels, 10, 8, 5, 3, mid);
        FillEllipse(pixels, 21, 7, 7, 5, outline);
        FillEllipse(pixels, 21, 8, 5, 3, dark);
        FillEllipse(pixels, 16, 13, 7, 6, outline);
        FillEllipse(pixels, 16, 14, 5, 4, mid);
        FillRect(pixels, 4, 2, 24, 3, outline);
    }

    private static void DrawHangingChain(Color32[] pixels)
    {
        Color32 outline = Hex("#242630");
        Color32 metal = Hex("#7E8791");
        Color32 shine = Hex("#B3BDC2");
        for (int i = 0; i < 5; i++)
        {
            int x = i % 2 == 0 ? 15 : 17;
            int y = 28 - i * 6;
            EllipseOutline(pixels, x, y, 4, 5, outline);
            EllipseOutline(pixels, x, y, 3, 4, metal);
            Set(pixels, x - 2, y + 2, shine);
        }
    }

    private static void DrawSkull(Color32[] pixels)
    {
        Color32 outline = Hex("#332F32");
        Color32 bone = Hex("#D8CFAD");
        Color32 shade = Hex("#A99F82");
        FillEllipse(pixels, 16, 11, 10, 9, outline);
        FillEllipse(pixels, 16, 12, 8, 7, bone);
        FillRect(pixels, 11, 4, 10, 5, outline);
        FillRect(pixels, 12, 5, 8, 4, bone);
        FillEllipse(pixels, 12, 13, 3, 3, outline);
        FillEllipse(pixels, 20, 13, 3, 3, outline);
        FillRect(pixels, 15, 8, 2, 3, shade);
        Set(pixels, 13, 5, outline);
        Set(pixels, 16, 5, outline);
        Set(pixels, 19, 5, outline);
    }

    private static void DrawCrossedBones(Color32[] pixels)
    {
        Color32 outline = Hex("#332F32");
        Color32 bone = Hex("#D8CFAD");
        ThickLine(pixels, 7, 5, 25, 17, outline);
        ThickLine(pixels, 7, 17, 25, 5, outline);
        Line(pixels, 8, 6, 24, 16, bone);
        Line(pixels, 8, 16, 24, 6, bone);
        foreach (Vector2Int end in new[]
        {
            new Vector2Int(7, 5), new Vector2Int(25, 17),
            new Vector2Int(7, 17), new Vector2Int(25, 5)
        })
        {
            FillEllipse(pixels, end.x, end.y, 3, 3, outline);
            FillEllipse(pixels, end.x, end.y, 2, 2, bone);
        }
    }

    private static void DrawPickaxe(Color32[] pixels)
    {
        Color32 outline = Hex("#29262A");
        Color32 wood = Hex("#795039");
        Color32 metal = Hex("#9DA7AC");
        ThickLine(pixels, 9, 3, 21, 22, outline);
        Line(pixels, 9, 3, 21, 22, wood);
        ThickLine(pixels, 10, 22, 27, 24, outline);
        Line(pixels, 10, 22, 27, 24, metal);
        Line(pixels, 9, 21, 5, 18, metal);
    }

    private static void DrawShovel(Color32[] pixels)
    {
        Color32 outline = Hex("#29262A");
        Color32 wood = Hex("#795039");
        Color32 metal = Hex("#929CA3");
        ThickLine(pixels, 8, 4, 23, 24, outline);
        Line(pixels, 8, 4, 23, 24, wood);
        FillEllipse(pixels, 7, 5, 5, 6, outline);
        FillEllipse(pixels, 7, 6, 3, 4, metal);
        ThickLine(pixels, 20, 25, 26, 25, outline);
        Line(pixels, 20, 25, 26, 25, wood);
    }

    private static void DrawHammer(Color32[] pixels)
    {
        Color32 outline = Hex("#29262A");
        Color32 wood = Hex("#795039");
        Color32 metal = Hex("#929CA3");
        ThickLine(pixels, 8, 4, 20, 20, outline);
        Line(pixels, 8, 4, 20, 20, wood);
        FillRect(pixels, 14, 19, 15, 7, outline);
        FillRect(pixels, 15, 20, 13, 5, metal);
        FillRect(pixels, 25, 21, 3, 3, Hex("#BBC3C6"));
    }

    private static void Set(Color32[] pixels, int x, int y, Color32 color)
    {
        if (x >= 0 && x < 32 && y >= 0 && y < 32)
        {
            pixels[y * 32 + x] = color;
        }
    }

    private static void FillRect(
        Color32[] pixels,
        int x,
        int y,
        int width,
        int height,
        Color32 color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                Set(pixels, px, py, color);
            }
        }
    }

    private static void FillEllipse(
        Color32[] pixels,
        int centerX,
        int centerY,
        int radiusX,
        int radiusY,
        Color32 color)
    {
        for (int y = -radiusY; y <= radiusY; y++)
        {
            for (int x = -radiusX; x <= radiusX; x++)
            {
                if ((x * x) / (float)(radiusX * radiusX) +
                    (y * y) / (float)(radiusY * radiusY) <= 1f)
                {
                    Set(pixels, centerX + x, centerY + y, color);
                }
            }
        }
    }

    private static void EllipseOutline(
        Color32[] pixels,
        int centerX,
        int centerY,
        int radiusX,
        int radiusY,
        Color32 color)
    {
        for (int angle = 0; angle < 360; angle += 10)
        {
            float radians = angle * Mathf.Deg2Rad;
            Set(
                pixels,
                centerX + Mathf.RoundToInt(Mathf.Cos(radians) * radiusX),
                centerY + Mathf.RoundToInt(Mathf.Sin(radians) * radiusY),
                color);
        }
    }

    private static void Line(
        Color32[] pixels,
        int x0,
        int y0,
        int x1,
        int y1,
        Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;
        while (true)
        {
            Set(pixels, x0, y0, color);
            if (x0 == x1 && y0 == y1)
            {
                break;
            }
            int twiceError = 2 * error;
            if (twiceError >= dy)
            {
                error += dy;
                x0 += sx;
            }
            if (twiceError <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private static void ThickLine(
        Color32[] pixels,
        int x0,
        int y0,
        int x1,
        int y1,
        Color32 color)
    {
        Line(pixels, x0, y0, x1, y1, color);
        Line(pixels, x0 + 1, y0, x1 + 1, y1, color);
        Line(pixels, x0, y0 + 1, x1, y1 + 1, color);
    }

    private static Color32 Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color color);
        return color;
    }

    private readonly struct PropSource
    {
        public readonly string name;
        public readonly string path;
        public readonly string spriteName;
        public readonly float scale;

        public PropSource(string name, string path, string spriteName, float scale)
        {
            this.name = name;
            this.path = path;
            this.spriteName = spriteName;
            this.scale = scale;
        }
    }

    private readonly struct LoadedProp
    {
        public readonly PropSource source;
        public readonly Sprite sprite;

        public LoadedProp(PropSource source, Sprite sprite)
        {
            this.source = source;
            this.sprite = sprite;
        }
    }
}
