#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DefaultExecutionOrder(100)]
public class GameUI : MonoBehaviour
{
    [Header("Yildiz Gorselleri")]
    [SerializeField] private Sprite emptyStarSprite;
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private float starSize = 48f;
    [SerializeField] private float starSpacing = 10f;
    [SerializeField] private Color filledStarColor = Color.white;
    [SerializeField] private Color emptyStarColor = new Color(0.62f, 0.64f, 0.68f, 1f);

    private Image[] starImages;

    void OnEnable()
    {
#if UNITY_EDITOR
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }
#endif

        LoadSpritesIfNeeded();

        if (!Application.isPlaying)
        {
            EnsureEditorPreview();
        }
    }

    void Awake()
    {
        if (Application.isPlaying)
        {
            LoadSpritesIfNeeded();
        }
    }

    void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        BuildStarDisplay();

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameUI: Sahnedeki GameManager objesi bulunamadi.");
            return;
        }

        GameManager.Instance.OnStarsChanged += HandleStarsChanged;
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        HandleStarsChanged(GameManager.Instance.CollectedStars, GameManager.Instance.MaxStarsPerLevel);
        HandleGameStateChanged(GameManager.Instance.CurrentState);
    }

    void OnDestroy()
    {
        if (!Application.isPlaying || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnStarsChanged -= HandleStarsChanged;
        GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void EnsureEditorPreview()
    {
        if (emptyStarSprite == null || filledStarSprite == null)
        {
            return;
        }

        HideLegacyStarText();

        Transform existingDisplay = transform.Find("StarDisplay");
        if (existingDisplay != null)
        {
            CacheStarImages(existingDisplay);
            ApplyStarVisuals(0);
            return;
        }

        CreateStarDisplay(starCount: 3, editorPreview: true);
        ApplyStarVisuals(0);
    }

    private void LoadSpritesIfNeeded()
    {
        if (emptyStarSprite != null && filledStarSprite != null)
        {
            return;
        }

        emptyStarSprite ??= StarUiSprites.GetEmptyStar();
        filledStarSprite ??= StarUiSprites.GetFilledStar();

        if (emptyStarSprite == null || filledStarSprite == null)
        {
            Debug.LogWarning("GameUI: Yildiz sprite'lari yuklenemedi.");
        }
    }

    private void BuildStarDisplay()
    {
        if (emptyStarSprite == null || filledStarSprite == null)
        {
            Debug.LogError("GameUI: Kenney Blue yildiz sprite'lari atanmamis.");
            return;
        }

        Transform existingDisplay = transform.Find("StarDisplay");
        if (existingDisplay != null)
        {
            DestroyObject(existingDisplay.gameObject);
        }

        HideLegacyStarText();

        int starCount = GameManager.Instance != null ? GameManager.Instance.MaxStarsPerLevel : 3;
        CreateStarDisplay(starCount, editorPreview: false);
    }

    private void CreateStarDisplay(int starCount, bool editorPreview)
    {
        Transform legacyStarText = transform.Find("StarText");

        GameObject container = new GameObject("StarDisplay", typeof(RectTransform));
        container.transform.SetParent(transform, false);

        if (editorPreview)
        {
            container.hideFlags = HideFlags.DontSave;
        }

        RectTransform containerRect = container.GetComponent<RectTransform>();
        if (legacyStarText is RectTransform legacyRect)
        {
            CopyRectTransform(legacyRect, containerRect);
        }
        else
        {
            containerRect.anchorMin = new Vector2(0f, 1f);
            containerRect.anchorMax = new Vector2(0f, 1f);
            containerRect.pivot = new Vector2(0f, 1f);
            containerRect.anchoredPosition = new Vector2(40f, -40f);
        }

        float totalWidth = starCount * starSize + (starCount - 1) * starSpacing;
        containerRect.sizeDelta = new Vector2(totalWidth, starSize);

        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = starSpacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        starImages = new Image[starCount];

        for (int i = 0; i < starCount; i++)
        {
            GameObject starObject = new GameObject($"Star_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            starObject.transform.SetParent(container.transform, false);

            if (editorPreview)
            {
                starObject.hideFlags = HideFlags.DontSave;
            }

            RectTransform starRect = starObject.GetComponent<RectTransform>();
            starRect.sizeDelta = new Vector2(starSize, starSize);

            Image image = starObject.GetComponent<Image>();
            image.sprite = emptyStarSprite;
            image.color = emptyStarColor;
            image.preserveAspect = true;
            image.raycastTarget = false;

            starImages[i] = image;
        }
    }

    private void HideLegacyStarText()
    {
        Transform legacyStarText = transform.Find("StarText");
        if (legacyStarText != null)
        {
            legacyStarText.gameObject.SetActive(false);
        }
    }

    private void CacheStarImages(Transform display)
    {
        starImages = display.GetComponentsInChildren<Image>(true);
    }

    private void ApplyStarVisuals(int collected)
    {
        if (starImages == null)
        {
            return;
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
            {
                continue;
            }

            bool isFilled = i < collected;
            starImages[i].sprite = isFilled ? filledStarSprite : emptyStarSprite;
            starImages[i].color = isFilled ? filledStarColor : emptyStarColor;
        }
    }

    private void HandleStarsChanged(int collected, int max)
    {
        ApplyStarVisuals(collected);
    }

    private void HandleGameStateChanged(GameState state)
    {
        // Menu, pause, level complete ekranlari burada yonetilebilir.
    }

    private void DestroyObject(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(obj);
            return;
        }
#endif
        Destroy(obj);
    }

    private static void CopyRectTransform(RectTransform source, RectTransform target)
    {
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.localRotation = source.localRotation;
        target.localScale = source.localScale;
    }
}
