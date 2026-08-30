using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class DayAtmosphereController
{
    private static readonly Color WarmDaylight = Hex("#FFF8E7");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToScene(scene, true);
    }

    public static bool ApplyToScene(Scene scene, bool createDecorativeOverlays)
    {
        if (!TryGetLevelNumber(scene.name, out int level))
        {
            return false;
        }

        GetTheme(
            level,
            out Color cameraColor,
            out Color backgroundTint,
            out Color tileTint,
            out float lightIntensity);

        foreach (Light2D light in Object.FindObjectsByType<Light2D>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (light.gameObject.scene == scene &&
                light.lightType == Light2D.LightType.Global)
            {
                light.color = WarmDaylight;
                light.intensity = lightIntensity;
            }
        }

        foreach (Camera camera in Object.FindObjectsByType<Camera>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (camera.gameObject.scene == scene)
            {
                camera.backgroundColor = cameraColor;
            }
        }

        foreach (Tilemap tilemap in Object.FindObjectsByType<Tilemap>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (tilemap.gameObject.scene == scene)
            {
                tilemap.color = tileTint;
            }
        }

        foreach (SpriteRenderer renderer in Object.FindObjectsByType<SpriteRenderer>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (renderer.gameObject.scene != scene)
            {
                continue;
            }

            if (renderer.name == "Background_0")
            {
                renderer.color = backgroundTint;
            }
            else if (level >= 5 && IsStructuralSprite(renderer.name))
            {
                renderer.color = new Color(1f, 0.97f, 0.84f, renderer.color.a);
            }
        }

        RenderSettings.ambientLight = Color.Lerp(cameraColor, WarmDaylight, 0.35f);

        if (createDecorativeOverlays && Application.isPlaying)
        {
            AddLocalHighlights(scene);

            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.gameObject.scene == scene)
            {
                if (level == 3 || level == 4)
                {
                    CreateCameraGradient(
                        mainCamera,
                        "CanyonSkyGradient",
                        new Color(0.28f, 0.68f, 0.95f, 0.10f),
                        new Color(0.84f, 0.94f, 1f, 0.22f),
                        -4);
                    CreateSunShaftLights(scene, level);
                }
                else if (level >= 5)
                {
                    CreateCameraGradient(
                        mainCamera,
                        "WarmSunReflections",
                        new Color(1f, 0.82f, 0.38f, 0.16f),
                        new Color(1f, 0.97f, 0.82f, 0.04f),
                        -4);
                }
            }
        }

        return true;
    }

    private static void AddLocalHighlights(Scene scene)
    {
        foreach (SpriteRenderer renderer in Object.FindObjectsByType<SpriteRenderer>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (renderer.gameObject.scene != scene)
            {
                continue;
            }

            if (renderer.CompareTag("Star"))
            {
                EnsurePointHighlight(
                    renderer.transform,
                    "Collectible Day Highlight",
                    new Color(1f, 0.88f, 0.35f, 1f),
                    0.22f,
                    1.7f);
            }
            else if (renderer.CompareTag("Finish"))
            {
                EnsurePointHighlight(
                    renderer.transform,
                    "Goal Day Highlight",
                    new Color(0.55f, 0.95f, 1f, 1f),
                    0.26f,
                    2.2f);
            }
            else if (renderer.tag == "Enemy" ||
                     renderer.name.ToLowerInvariant().Contains("enemy"))
            {
                EnsurePointHighlight(
                    renderer.transform,
                    "Enemy Day Highlight",
                    new Color(1f, 0.48f, 0.25f, 1f),
                    0.20f,
                    1.6f);
            }
            else if (renderer.CompareTag("Trap"))
            {
                EnsurePointHighlight(
                    renderer.transform,
                    "Hazard Day Highlight",
                    new Color(1f, 0.55f, 0.30f, 1f),
                    0.15f,
                    1.35f);
            }
        }
    }

    private static void EnsurePointHighlight(
        Transform parent,
        string lightName,
        Color color,
        float intensity,
        float radius)
    {
        Transform existing = parent.Find(lightName);
        if (existing != null)
        {
            return;
        }

        var lightObject = new GameObject(lightName)
        {
            hideFlags = HideFlags.DontSave
        };
        lightObject.transform.SetParent(parent, false);

        Light2D light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.pointLightInnerRadius = radius * 0.3f;
        light.pointLightOuterRadius = radius;
        light.falloffIntensity = 0.65f;
    }

    private static void CreateSunShaftLights(Scene scene, int level)
    {
        if (GameObject.Find("Day Sun Shafts") != null)
        {
            return;
        }

        var root = new GameObject("Day Sun Shafts")
        {
            hideFlags = HideFlags.DontSave
        };
        SceneManager.MoveGameObjectToScene(root, scene);

        float finishX = level == 3 ? 76f : 122f;
        for (int column = 0; column < 4; column++)
        {
            float x = Mathf.Lerp(16f, finishX - 12f, column / 3f);
            for (int row = 0; row < 3; row++)
            {
                var lightObject = new GameObject($"Sun Shaft {column + 1}-{row + 1}");
                lightObject.transform.SetParent(root.transform, false);
                lightObject.transform.position = new Vector3(x, 47f - row * 4f, 0f);

                Light2D light = lightObject.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Point;
                light.color = new Color(1f, 0.88f, 0.48f, 1f);
                light.intensity = 0.10f;
                light.pointLightInnerRadius = 1.2f;
                light.pointLightOuterRadius = 4.5f;
                light.falloffIntensity = 0.75f;
            }
        }
    }

    private static void GetTheme(
        int level,
        out Color cameraColor,
        out Color backgroundTint,
        out Color tileTint,
        out float lightIntensity)
    {
        if (level <= 2)
        {
            cameraColor = Hex("#A9DFF2");
            backgroundTint = Hex("#F2FFE7");
            tileTint = Hex("#FFF4D8");
            lightIntensity = 1.10f;
        }
        else if (level <= 4)
        {
            cameraColor = Hex("#79C9F2");
            backgroundTint = Hex("#DCEFFF");
            tileTint = Hex("#FFE6BF");
            lightIntensity = 1.08f;
        }
        else
        {
            cameraColor = Hex("#9DDBF4");
            backgroundTint = Hex("#EDF7FF");
            tileTint = Hex("#FFF1CF");
            lightIntensity = 1.18f;
        }
    }

    private static void CreateCameraGradient(
        Camera camera,
        string objectName,
        Color top,
        Color bottom,
        int sortingOrder)
    {
        Transform existing = camera.transform.Find(objectName);
        if (existing != null)
        {
            return;
        }

        const int gradientHeight = 64;
        var texture = new Texture2D(1, gradientHeight, TextureFormat.RGBA32, false)
        {
            name = objectName + "Texture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };

        for (int y = 0; y < gradientHeight; y++)
        {
            texture.SetPixel(0, y, Color.Lerp(bottom, top, y / (gradientHeight - 1f)));
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, gradientHeight),
            new Vector2(0.5f, 0.5f),
            1f);
        sprite.name = objectName + "Sprite";
        sprite.hideFlags = HideFlags.DontSave;

        var overlay = new GameObject(objectName)
        {
            hideFlags = HideFlags.DontSave
        };
        overlay.transform.SetParent(camera.transform, false);
        overlay.transform.localPosition = new Vector3(0f, 0f, 10f);

        SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;

        float height = camera.orthographicSize * 2.2f;
        float width = height * camera.aspect;
        overlay.transform.localScale = new Vector3(width, height / gradientHeight, 1f);
    }

    private static bool IsStructuralSprite(string objectName)
    {
        string lowerName = objectName.ToLowerInvariant();
        return lowerName.Contains("building") ||
               lowerName.Contains("metal") ||
               lowerName.Contains("platform") ||
               lowerName.Contains("structure");
    }

    private static bool TryGetLevelNumber(string sceneName, out int level)
    {
        level = 0;
        return sceneName.StartsWith("Level_") &&
               int.TryParse(sceneName.Substring("Level_".Length), out level) &&
               level >= 1 &&
               level <= 6;
    }

    private static Color Hex(string value)
    {
        return ColorUtility.TryParseHtmlString(value, out Color color)
            ? color
            : Color.white;
    }
}
