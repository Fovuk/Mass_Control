using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class LevelCompletePanelSetup
{
    const string EmptyStarPath = "Assets/kenney_ui-pack/PNG/Blue/Default/star_outline.png";
    const string FilledStarPath = "Assets/kenney_ui-pack/PNG/Blue/Default/star.png";

    static readonly string[] LevelScenePaths =
    {
        "Assets/Scenes/Level_01.unity",
        "Assets/Scenes/Level_02.unity",
        "Assets/Scenes/Level_03.unity",
        "Assets/Scenes/Level_04.unity",
        "Assets/Scenes/Level_05.unity",
        "Assets/Scenes/Level_06.unity",
    };

    [MenuItem("Tools/Setup Level Complete Star Display")]
    public static void SetupAllLevelCompletePanels()
    {
        Sprite emptyStar = AssetDatabase.LoadAssetAtPath<Sprite>(EmptyStarPath);
        Sprite filledStar = AssetDatabase.LoadAssetAtPath<Sprite>(FilledStarPath);

        if (emptyStar == null || filledStar == null)
        {
            Debug.LogError("[LevelCompletePanelSetup] Yildiz sprite'lari bulunamadi.");
            return;
        }

        foreach (string scenePath in LevelScenePaths)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"[LevelCompletePanelSetup] Sahne bulunamadi: {scenePath}");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!SetupCurrentScene(emptyStar, filledStar))
            {
                Debug.LogWarning($"[LevelCompletePanelSetup] Kurulum atlandi: {scenePath}");
                continue;
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[LevelCompletePanelSetup] Kuruldu: {scenePath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[LevelCompletePanelSetup] Tum level sahneleri guncellendi.");
    }

    [MenuItem("Tools/Setup Active Level Complete Star Display")]
    public static void SetupActiveScene()
    {
        Sprite emptyStar = AssetDatabase.LoadAssetAtPath<Sprite>(EmptyStarPath);
        Sprite filledStar = AssetDatabase.LoadAssetAtPath<Sprite>(FilledStarPath);

        if (emptyStar == null || filledStar == null)
        {
            Debug.LogError("[LevelCompletePanelSetup] Yildiz sprite'lari bulunamadi.");
            return;
        }

        if (SetupCurrentScene(emptyStar, filledStar))
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[LevelCompletePanelSetup] Aktif sahne guncellendi.");
        }
    }

    static bool SetupCurrentScene(Sprite emptyStar, Sprite filledStar)
    {
        LevelCompleteUI completeUi = Object.FindFirstObjectByType<LevelCompleteUI>(FindObjectsInactive.Include);
        if (completeUi == null)
        {
            Debug.LogError("[LevelCompletePanelSetup] LevelCompleteUI bulunamadi.");
            return false;
        }

        Transform completeCard = FindCompleteCard(completeUi.transform);
        if (completeCard == null)
        {
            Debug.LogError("[LevelCompletePanelSetup] CompleteCard bulunamadi.");
            return false;
        }

        Transform runtimeDisplay = completeCard.Find("CompleteStarDisplay");
        if (runtimeDisplay != null)
        {
            Object.DestroyImmediate(runtimeDisplay.gameObject);
        }

        Image[] starImages = EnsureStarDisplay(completeCard, emptyStar);
        ConfigureSummaryText(completeUi);
        WireLevelCompleteUi(completeUi, starImages, emptyStar, filledStar);

        return true;
    }

    static Transform FindCompleteCard(Transform uiRoot)
    {
        Transform panel = uiRoot.Find("LevelCompletePanel");
        if (panel != null)
        {
            Transform card = panel.Find("CompleteCard");
            if (card != null)
            {
                return card;
            }
        }

        foreach (Transform child in uiRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "CompleteCard")
            {
                return child;
            }
        }

        return null;
    }

    static Image[] EnsureStarDisplay(Transform completeCard, Sprite emptyStar)
    {
        Transform displayRoot = completeCard.Find("StarDisplay");
        GameObject displayObject;

        if (displayRoot == null)
        {
            displayObject = new GameObject("StarDisplay", typeof(RectTransform));
            displayObject.transform.SetParent(completeCard, false);

            Transform title = completeCard.Find("Title");
            int siblingIndex = title != null ? title.GetSiblingIndex() + 1 : 1;
            displayObject.transform.SetSiblingIndex(siblingIndex);
        }
        else
        {
            displayObject = displayRoot.gameObject;
        }

        RectTransform displayRect = displayObject.GetComponent<RectTransform>();
        displayRect.anchorMin = new Vector2(0.5f, 0.5f);
        displayRect.anchorMax = new Vector2(0.5f, 0.5f);
        displayRect.pivot = new Vector2(0.5f, 0.5f);
        displayRect.anchoredPosition = new Vector2(0f, 90f);

        const float starSize = 48f;
        const float starSpacing = 10f;
        const int starCount = 3;
        displayRect.sizeDelta = new Vector2(
            starCount * starSize + (starCount - 1) * starSpacing,
            starSize);

        HorizontalLayoutGroup layout = displayObject.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = displayObject.AddComponent<HorizontalLayoutGroup>();
        }

        layout.spacing = starSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Image[] starImages = new Image[starCount];
        Color emptyColor = new Color(0.62f, 0.64f, 0.68f, 1f);

        for (int i = 0; i < starCount; i++)
        {
            string starName = $"Star_{i + 1}";
            Transform starTransform = displayObject.transform.Find(starName);
            GameObject starObject;

            if (starTransform == null)
            {
                starObject = new GameObject(starName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                starObject.transform.SetParent(displayObject.transform, false);
            }
            else
            {
                starObject = starTransform.gameObject;
            }

            RectTransform starRect = starObject.GetComponent<RectTransform>();
            starRect.sizeDelta = new Vector2(starSize, starSize);

            Image image = starObject.GetComponent<Image>();
            image.sprite = emptyStar;
            image.color = emptyColor;
            image.preserveAspect = true;
            image.raycastTarget = false;

            starImages[i] = image;
        }

        return starImages;
    }

    static void ConfigureSummaryText(LevelCompleteUI completeUi)
    {
        SerializedObject uiObject = new SerializedObject(completeUi);
        SerializedProperty summaryTextProperty = uiObject.FindProperty("summaryText");
        if (summaryTextProperty == null || summaryTextProperty.objectReferenceValue == null)
        {
            return;
        }

        var summaryText = summaryTextProperty.objectReferenceValue as TMPro.TextMeshProUGUI;
        if (summaryText == null)
        {
            return;
        }

        summaryText.text = string.Empty;
        summaryText.gameObject.SetActive(false);
    }

    static void WireLevelCompleteUi(LevelCompleteUI completeUi, Image[] starImages, Sprite emptyStar, Sprite filledStar)
    {
        SerializedObject uiObject = new SerializedObject(completeUi);

        SerializedProperty starImagesProperty = uiObject.FindProperty("starImages");
        starImagesProperty.arraySize = starImages.Length;
        for (int i = 0; i < starImages.Length; i++)
        {
            starImagesProperty.GetArrayElementAtIndex(i).objectReferenceValue = starImages[i];
        }

        uiObject.FindProperty("emptyStarSprite").objectReferenceValue = emptyStar;
        uiObject.FindProperty("filledStarSprite").objectReferenceValue = filledStar;
        uiObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(completeUi);
    }
}
