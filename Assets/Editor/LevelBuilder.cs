using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class LevelBuilder
{
    const string TemplateScene = "Assets/Scenes/Level_03.unity";
    const string GrassTilePath =
        "Assets/Free Platform Game Assets/Update 1.9/New Tiles (2D view)/Spring/128x128/Grass_0.asset";
    const string GrassMidTilePath =
        "Assets/Free Platform Game Assets/Platform Game Assets/Tiles/png/128x128/GrassMid_0.asset";

    static readonly string[] StripNames =
    {
        "Grid", "Grid (1)", "Spikes", "Stars", "Moveable_Object", "MoveableObject", "FinishPortal", "Ground",
        "Spike", "Spike (1)", "Star", "Star (1)", "Star (2)", "Star (3)", "Tilemap",
    };

    static readonly string[] StripPrefixes = { "Moving_Spike", "Triangle", "Moveable_Object", "MoveableObject" };

    [MenuItem("Tools/Build Levels 3-6")]
    public static void BuildAllLevelsMenu() => BuildAllLevels();

    [MenuItem("Tools/Build Levels 4-6")]
    public static void BuildLevels456Menu() => BuildLevels456();

    public static void BuildAllLevels()
    {
        for (var i = 3; i <= 6; i++)
            BuildLevel(i);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LevelBuilder] Finished building levels 3-6.");
    }

    public static void BuildLevels456()
    {
        for (var i = 4; i <= 6; i++)
            BuildLevel(i);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LevelBuilder] Finished building levels 4-6.");
    }

    public static void BuildLevel(int levelNumber)
    {
        var def = LevelDefinitions.Get(levelNumber);
        var targetPath = $"Assets/Scenes/Level_{levelNumber:D2}.unity";

        if (!File.Exists(TemplateScene))
        {
            Debug.LogError($"[LevelBuilder] Template scene missing: {TemplateScene}");
            return;
        }

        File.Copy(TemplateScene, targetPath, true);
        AssetDatabase.ImportAsset(targetPath);

        var scene = EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Single);
        StripGameplayObjects();
        BuildTilemap(def);
        PlaceGameplayObjects(def);
        AlignCamera(def.player);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[LevelBuilder] Built Level {levelNumber:D2}: {def.name} ({CountTiles(def)} tiles)");
    }

    static void StripGameplayObjects()
    {
        var toDestroy = new List<GameObject>();

        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            CollectForRemoval(root, toDestroy);

        foreach (var go in toDestroy)
            Object.DestroyImmediate(go);
    }

    static void CollectForRemoval(GameObject go, List<GameObject> toDestroy)
    {
        if (ShouldStrip(go.name))
        {
            toDestroy.Add(go);
            return;
        }

        for (var i = go.transform.childCount - 1; i >= 0; i--)
            CollectForRemoval(go.transform.GetChild(i).gameObject, toDestroy);
    }

    static bool ShouldStrip(string name)
    {
        foreach (var exact in StripNames)
        {
            if (name == exact)
                return true;
        }

        foreach (var prefix in StripPrefixes)
        {
            if (name.StartsWith(prefix))
                return true;
        }

        return false;
    }

    static void BuildTilemap(LevelDefinitions.LevelDef def)
    {
        var grassTile = AssetDatabase.LoadAssetAtPath<TileBase>(GrassTilePath);
        var grassMidTile = AssetDatabase.LoadAssetAtPath<TileBase>(GrassMidTilePath);
        if (grassTile == null)
        {
            Debug.LogError($"[LevelBuilder] Grass tile not found at {GrassTilePath}");
            return;
        }

        if (grassMidTile == null)
            grassMidTile = grassTile;

        var gridGo = new GameObject("Grid");
        gridGo.layer = LayerMask.NameToLayer("Ground");
        gridGo.transform.position = new Vector3(0f, LevelDefinitions.GridWorldY, 0f);

        var grid = gridGo.AddComponent<Grid>();
        grid.cellSize = new Vector3(1f, 1f, 0f);

        var tilemapGo = new GameObject("Tilemap");
        tilemapGo.layer = LayerMask.NameToLayer("Ground");
        tilemapGo.transform.SetParent(gridGo.transform, false);
        tilemapGo.transform.localPosition = new Vector3(0f, LevelDefinitions.TilemapLocalY, 0f);

        var tilemap = tilemapGo.AddComponent<Tilemap>();
        tilemapGo.AddComponent<TilemapRenderer>();
        tilemapGo.AddComponent<TilemapCollider2D>();

        var rows = def.map;
        for (var row = 0; row < rows.Length; row++)
        {
            var line = rows[row];
            for (var col = 0; col < line.Length; col++)
            {
                var ch = line[col];
                if (ch != '#' && ch != '=')
                    continue;

                var cell = LevelDefinitions.CellFromMap(def.originX, row, col, rows.Length);
                tilemap.SetTile(cell, ch == '=' ? grassMidTile : grassTile);
            }
        }
    }

    static void PlaceGameplayObjects(LevelDefinitions.LevelDef def)
    {
        var starsParent = new GameObject("Stars");
        var spikesParent = new GameObject("Spikes");

        PlacePlayer(def.player);
        PlaceFinish(def.finish);

        for (var i = 0; i < def.stars.Length; i++)
            PlaceStar(starsParent.transform, def.stars[i], i);

        if (def.pushable.HasValue)
            PlacePushable(def.pushable.Value);

        foreach (var pos in def.staticSpikes)
            PlaceStaticSpike(spikesParent.transform, pos);

        for (var i = 0; i < def.movingSpikes.Length; i++)
            PlaceMovingSpike(spikesParent.transform, def.movingSpikes[i], i + 1);
    }

    static void PlacePlayer(Vector2 pos)
    {
        var player = FindNamedRoot("Player");
        if (player == null)
        {
            Debug.LogError("[LevelBuilder] Player object not found in template scene.");
            return;
        }

        player.transform.position = new Vector3(pos.x, pos.y, 0f);
    }

    static void PlaceFinish(Vector2 pos)
    {
        var finish = CreateFinishPortal();
        finish.transform.position = new Vector3(pos.x, pos.y, 0f);
    }

    static GameObject CreateFinishPortal()
    {
        var go = new GameObject("FinishPortal");
        go.tag = "Finish";

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Sprites/FinishPortal.png");
        if (sr.sprite == null)
            sr.sprite = LoadSpriteByGuid("311925a002f4447b3a28927169b83ea6");
        sr.color = new Color(0f, 0.86f, 1f, 1f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        go.AddComponent<FinishPortal>();
        return go;
    }

    static void PlaceStar(Transform parent, Vector2 pos, int index)
    {
        var go = new GameObject(index == 0 ? "Star" : $"Star ({index})");
        go.tag = "Star";
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0.2f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSpriteByGuid("cf83d3e6f5530114c8a312261f2f748d");
        sr.color = new Color(1f, 0.98f, 0.27f, 1f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.88f, 1f);

        go.AddComponent<CollectibleStarSpin>();
    }

    const string MoveableObjectPrefabPath = "Assets/Prefabs/MoveableObject.prefab";

    static void PlacePushable(Vector2 pos)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MoveableObjectPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[LevelBuilder] Missing prefab at {MoveableObjectPrefabPath}. Run Tools/Moveable Object/Create Or Update Prefab first.");
            return;
        }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = "MoveableObject";
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
    }

    static void PlaceStaticSpike(Transform parent, Vector2 pos)
    {
        var go = new GameObject("Spike");
        go.tag = "Trap";
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSpriteByGuid("15562fffe2efc534bb9cb4cdb86aa490");
        sr.color = new Color(1f, 0.48f, 0.48f, 1f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.5f, 1f);
    }

    static void PlaceMovingSpike(Transform parent, LevelDefinitions.MovingSpikeDef def, int index)
    {
        var go = new GameObject($"Moving_Spike ({index})");
        go.tag = "Trap";
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(def.position.x, def.position.y, 0f);
        go.transform.localScale = new Vector3(1.32f, 1.09f, 1f);

        var pointA = new GameObject("PointA");
        pointA.transform.SetParent(go.transform, false);
        pointA.transform.position = new Vector3(def.pointA.x, def.pointA.y, 0f);

        var pointB = new GameObject("PointB");
        pointB.transform.SetParent(go.transform, false);
        pointB.transform.position = new Vector3(def.pointB.x, def.pointB.y, 0f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.offset = new Vector2(0f, 0.1f);
        col.size = new Vector2(2.34f, 2.27f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("Assets/Free Platform Game Assets/Platform Game Assets/Enemies/png/256x256/Mace.png");
        sr.color = new Color(1f, 0.48f, 0.48f, 1f);

        var mover = go.AddComponent<MovingPlatform>();
        var so = new SerializedObject(mover);
        so.FindProperty("pointA").objectReferenceValue = pointA.transform;
        so.FindProperty("pointB").objectReferenceValue = pointB.transform;
        so.FindProperty("speed").floatValue = def.speed;
        so.FindProperty("startAtPointA").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AlignCamera(Vector2 playerPos)
    {
        var cam = FindNamedRoot("CinemachineCamera");
        if (cam != null)
            cam.transform.position = new Vector3(playerPos.x, playerPos.y, -10f);

        var mainCam = Camera.main;
        if (mainCam != null)
            mainCam.transform.position = new Vector3(playerPos.x, playerPos.y, -10f);
    }

    static GameObject FindNamedRoot(string name)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == name)
                return root;
        }

        return null;
    }

    static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite LoadSpriteByGuid(string guid)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static int CountTiles(LevelDefinitions.LevelDef def)
    {
        var count = 0;
        foreach (var row in def.map)
        {
            foreach (var ch in row)
            {
                if (ch == '#' || ch == '=')
                    count++;
            }
        }
        return count;
    }
}
