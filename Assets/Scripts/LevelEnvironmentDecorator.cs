using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class LevelEnvironmentDecorator
{
    private const string LibraryResourceName = "EnvironmentPropLibrary";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= Decorate;
        SceneManager.sceneLoaded += Decorate;
    }

    private static void Decorate(Scene scene, LoadSceneMode mode)
    {
        if (!TryGetLevel(scene.name, out int level) ||
            GameObject.Find("Runtime Environment Props") != null)
        {
            return;
        }

        EnvironmentPropLibrary library =
            Resources.Load<EnvironmentPropLibrary>(LibraryResourceName);
        if (library == null)
        {
            Debug.LogWarning("[LevelEnvironmentDecorator] Prop library bulunamadi.");
            return;
        }

        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        FinishPortal finish = Object.FindAnyObjectByType<FinishPortal>();
        float startX = player != null ? player.transform.position.x : 4f;
        float finishX = finish != null
            ? finish.transform.position.x
            : DefaultFinishX(level);
        float fallbackGroundY = player != null
            ? player.transform.position.y - 0.5f
            : 27.5f;

        var root = new GameObject("Runtime Environment Props")
        {
            hideFlags = HideFlags.DontSave
        };
        SceneManager.MoveGameObjectToScene(root, scene);

        int propCount = level <= 2 ? 7 : level <= 4 ? 8 : level == 6 ? 14 : 10;
        for (int i = 0; i < propCount; i++)
        {
            float normalized = (i + 1f) / (propCount + 1f);
            float x = Mathf.Lerp(startX + 5f, finishX - 5f, normalized);
            Sprite sprite = SelectSprite(library, level, i);
            if (sprite == null)
            {
                continue;
            }

            float groundY = FindGroundY(x, fallbackGroundY);
            CreateProp(
                root.transform,
                sprite,
                x,
                groundY,
                level,
                i);
        }
    }

    private static Sprite SelectSprite(
        EnvironmentPropLibrary library,
        int level,
        int index)
    {
        Sprite[] themedProps = level == 6
            ? library.level6Props
            : level <= 2
                ? library.meadowProps
                : level <= 4
                    ? library.canyonProps
                    : library.settlementProps;

        if (themedProps != null && themedProps.Length > 0)
        {
            return themedProps[index % themedProps.Length];
        }

        if (level <= 2)
        {
            Sprite[] meadow =
            {
                library.tree,
                library.woodPile,
                library.tree,
                library.house,
                library.woodPile
            };
            return meadow[index % meadow.Length];
        }

        if (level <= 4)
        {
            Sprite[] canyon =
            {
                library.woodPile,
                library.tree,
                library.woodPile,
                library.streetLamp
            };
            return canyon[index % canyon.Length];
        }

        Sprite[] settlement =
        {
            library.streetLamp,
            library.woodPile,
            library.house,
            library.streetLamp,
            library.woodPile
        };
        return settlement[index % settlement.Length];
    }

    private static void CreateProp(
        Transform parent,
        Sprite sprite,
        float x,
        float groundY,
        int level,
        int index)
    {
        var prop = new GameObject($"Decoration_{level:D2}_{index + 1:D2}_{sprite.name}");
        prop.transform.SetParent(parent, false);
        float propScale = GetPropScale(sprite.name);
        prop.transform.localScale = Vector3.one * propScale;
        bool isHangingChain = sprite.name.StartsWith("HangingChain");
        float y = isHangingChain
            ? groundY + 5f - sprite.bounds.max.y * propScale
            : groundY - sprite.bounds.min.y * propScale;
        prop.transform.position = new Vector3(x, y, 0f);

        SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = -1;

        bool isLamp = sprite.name.ToLowerInvariant().Contains("lamp");
        if (isLamp)
        {
            AddLampGlow(prop.transform, sprite.bounds.max.y * 0.78f);
        }
    }

    private static float GetPropScale(string spriteName)
    {
        if (spriteName == "Tree_0" ||
            spriteName == "Autumn Tree_0" ||
            spriteName == "Winter Tree_0")
        {
            return 0.6f;
        }
        if (spriteName == "ObjHouse")
        {
            return 4f;
        }
        if (spriteName == "Asset 4_0")
        {
            return 1.4f;
        }
        if (spriteName == "Asset 8_0" || spriteName == "Asset 10_0")
        {
            return 2f;
        }
        if (spriteName == "Asset 14_0")
        {
            return 1.8f;
        }
        if (spriteName == "Asset 15_0")
        {
            return 1.6f;
        }
        if (spriteName.StartsWith("Flower-"))
        {
            return 0.9f;
        }
        return 1f;
    }

    private static void AddLampGlow(Transform lamp, float localY)
    {
        var glowObject = new GameObject("Lamp Glow");
        glowObject.transform.SetParent(lamp, false);
        glowObject.transform.localPosition = new Vector3(0f, localY, 0f);

        Light2D glow = glowObject.AddComponent<Light2D>();
        glow.lightType = Light2D.LightType.Point;
        glow.color = new Color(1f, 0.78f, 0.34f, 1f);
        glow.intensity = 0.22f;
        glow.pointLightInnerRadius = 0.35f;
        glow.pointLightOuterRadius = 1.8f;
        glow.falloffIntensity = 0.7f;
    }

    private static float FindGroundY(float x, float fallback)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            new Vector2(x, 90f),
            Vector2.down,
            140f);

        int groundLayer = LayerMask.NameToLayer("Ground");
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger)
            {
                continue;
            }

            if (hit.collider.gameObject.layer == groundLayer ||
                hit.collider.GetComponent<TilemapCollider2D>() != null)
            {
                return hit.point.y;
            }
        }

        return fallback;
    }

    private static bool TryGetLevel(string sceneName, out int level)
    {
        level = 0;
        return sceneName.StartsWith("Level_") &&
               int.TryParse(sceneName.Substring("Level_".Length), out level) &&
               level >= 1 &&
               level <= 6;
    }

    private static float DefaultFinishX(int level)
    {
        return level switch
        {
            1 => 80f,
            2 => 100f,
            3 => 76f,
            4 => 122f,
            5 => 156f,
            _ => 196f
        };
    }
}
