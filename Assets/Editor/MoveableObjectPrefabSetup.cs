using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates/updates Assets/Prefabs/MoveableObject.prefab and replaces
/// every scene Moveable_Object* with a prefab instance (position kept).
/// </summary>
public static class MoveableObjectPrefabSetup
{
    const string PrefabPath = "Assets/Prefabs/MoveableObject.prefab";
    const string SpritePath = "Assets/Sprites/PushableBoulder_Level01.png";

    static readonly string[] LevelScenes =
    {
        "Assets/Scenes/Level_01.unity",
        "Assets/Scenes/Level_02.unity",
        "Assets/Scenes/Level_03.unity",
        "Assets/Scenes/Level_04.unity",
        "Assets/Scenes/Level_05.unity",
        "Assets/Scenes/Level_06.unity",
    };

    [MenuItem("Tools/Moveable Object/Create Or Update Prefab")]
    public static void CreateOrUpdatePrefabMenu()
    {
        CreateOrUpdatePrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[MoveableObjectPrefabSetup] Prefab ready at {PrefabPath}");
    }

    [MenuItem("Tools/Moveable Object/Convert All Levels To Prefab")]
    public static void ConvertAllLevelsMenu()
    {
        var prefab = CreateOrUpdatePrefab();
        if (prefab == null)
        {
            return;
        }

        int converted = 0;
        string previousScene = SceneManager.GetActiveScene().path;

        foreach (var scenePath in LevelScenes)
        {
            if (!File.Exists(scenePath))
            {
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            converted += ConvertSceneInstances(prefab);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(previousScene) && File.Exists(previousScene))
        {
            EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MoveableObjectPrefabSetup] Converted {converted} Moveable_Object(s) across levels to prefab instances.");
    }

    public static GameObject CreateOrUpdatePrefab()
    {
        EnsurePrefabsFolder();

        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        GameObject root;

        if (existing != null)
        {
            root = PrefabUtility.LoadPrefabContents(PrefabPath);
        }
        else
        {
            root = new GameObject("MoveableObject");
        }

        try
        {
            ConfigureRoot(root);
            var saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            return saved;
        }
        finally
        {
            if (existing != null)
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            else
            {
                Object.DestroyImmediate(root);
            }
        }
    }

    static void EnsurePrefabsFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
    }

    static void ConfigureRoot(GameObject root)
    {
        root.name = "MoveableObject";
        root.tag = "Pushable";
        root.layer = 0;

        var transform = root.transform;
        transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);

        var sr = root.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = root.AddComponent<SpriteRenderer>();
        }

        sr.sprite = LoadBoulderSprite();
        sr.color = Color.white;
        sr.sortingOrder = 0;

        var rb = root.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = root.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = 12f;
        rb.gravityScale = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.None;

        var circle = root.GetComponent<CircleCollider2D>();
        if (circle == null)
        {
            circle = root.AddComponent<CircleCollider2D>();
        }

        circle.offset = Vector2.zero;
        circle.radius = 4.8f;
        circle.isTrigger = false;

        // Remove extra colliders so every level shares one circle.
        foreach (var col in root.GetComponents<Collider2D>())
        {
            if (col != null && col != circle)
            {
                Object.DestroyImmediate(col);
            }
        }

        if (root.GetComponent<PushableObject>() == null)
        {
            root.AddComponent<PushableObject>();
        }
    }

    static Sprite LoadBoulderSprite()
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(SpritePath);
        foreach (var asset in sprites)
        {
            if (asset is Sprite sprite)
            {
                return sprite;
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
    }

    static int ConvertSceneInstances(GameObject prefab)
    {
        var toReplace = new List<GameObject>();
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            CollectMoveables(root, toReplace);
        }

        int count = 0;
        foreach (var old in toReplace)
        {
            if (PrefabUtility.GetCorrespondingObjectFromSource(old) == prefab)
            {
                continue;
            }

            var worldPos = old.transform.position;
            var parent = old.transform.parent;
            var sibling = old.transform.GetSiblingIndex();
            var active = old.activeSelf;

            Object.DestroyImmediate(old);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "MoveableObject";
            instance.transform.SetParent(parent, true);
            instance.transform.position = worldPos;
            instance.transform.SetSiblingIndex(sibling);
            instance.SetActive(active);
            count++;
        }

        return count;
    }

    static void CollectMoveables(GameObject go, List<GameObject> results)
    {
        if (IsMoveableName(go.name))
        {
            results.Add(go);
            return;
        }

        for (int i = 0; i < go.transform.childCount; i++)
        {
            CollectMoveables(go.transform.GetChild(i).gameObject, results);
        }
    }

    static bool IsMoveableName(string name)
    {
        return name == "MoveableObject"
            || name == "Moveable_Object"
            || name.StartsWith("Moveable_Object (")
            || name.StartsWith("MoveableObject (");
    }
}
