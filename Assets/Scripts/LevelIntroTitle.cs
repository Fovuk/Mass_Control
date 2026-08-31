using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelIntroTitle : MonoBehaviour
{
    [SerializeField] private float startDelay = 0.9f;
    [SerializeField] private float fadeInDuration = 0.65f;
    [SerializeField] private float holdDuration = 1.35f;
    [SerializeField] private float fadeOutDuration = 0.75f;
    [SerializeField] private int overlaySortOrder = 4950;

    private Canvas overlayCanvas;
    private CanvasGroup rootGroup;
    private RectTransform contentRoot;
    private TextMeshProUGUI subtitleLabel;
    private TextMeshProUGUI titleLabel;
    private TextMeshProUGUI titleShadowLabel;
    private Image backdrop;
    private Coroutine introRoutine;

    public static void EnsureOn(GameManager manager)
    {
        if (manager == null || manager.GetComponent<LevelIntroTitle>() != null)
        {
            return;
        }

        manager.gameObject.AddComponent<LevelIntroTitle>();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex <= 0)
        {
            return;
        }

        EnsureOverlay();
        ApplyContent();

        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
        }

        introRoutine = StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        overlayCanvas.gameObject.SetActive(true);
        SetVisualState(0f, 28f, 0.9f);

        if (startDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(startDelay);
        }

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            float eased = EaseOutCubic(t);
            SetVisualState(eased, Mathf.Lerp(28f, 0f, eased), Mathf.Lerp(0.9f, 1f, EaseOutBack(t)));
            yield return null;
        }

        SetVisualState(1f, 0f, 1f);

        float holdElapsed = 0f;
        while (holdElapsed < holdDuration)
        {
            holdElapsed += Time.unscaledDeltaTime;
            float pulse = 1f + Mathf.Sin(holdElapsed * 4.2f) * 0.012f;
            contentRoot.localScale = Vector3.one * pulse;
            yield return null;
        }

        contentRoot.localScale = Vector3.one;
        elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float eased = EaseInCubic(t);
            SetVisualState(1f - eased, Mathf.Lerp(0f, -22f, eased), Mathf.Lerp(1f, 0.96f, eased));
            yield return null;
        }

        overlayCanvas.gameObject.SetActive(false);
        introRoutine = null;
    }

    private void SetVisualState(float alpha, float offsetY, float scale)
    {
        if (rootGroup != null)
        {
            rootGroup.alpha = alpha;
        }

        if (contentRoot != null)
        {
            contentRoot.anchoredPosition = new Vector2(0f, offsetY);
            contentRoot.localScale = Vector3.one * scale;
        }
    }

    private void ApplyContent()
    {
        LocalizationManager.EnsureInstance();

        int levelNumber = SceneManager.GetActiveScene().buildIndex;
        string subtitle = LocalizationManager.Format("level_intro_format", levelNumber);
        string title = LocalizationManager.Get($"level_name_{levelNumber}");

        ApplyFonts();
        subtitleLabel.text = subtitle;
        titleLabel.text = title;
        titleShadowLabel.text = title;
    }

    private void ApplyFonts()
    {
        LevelIntroSettings settings = LevelIntroSettings.Get();
        TMP_FontAsset fallback = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        TMP_FontAsset titleFont = settings != null && settings.titleFont != null
            ? settings.titleFont
            : fallback;
        TMP_FontAsset subtitleFont = settings != null && settings.subtitleFont != null
            ? settings.subtitleFont
            : fallback;

        if (titleFont != null)
        {
            titleLabel.font = titleFont;
            titleShadowLabel.font = titleFont;
        }

        if (subtitleFont != null)
        {
            subtitleLabel.font = subtitleFont;
        }

        if (LocalizationManager.Instance != null &&
            LocalizationManager.Instance.CurrentLanguage == GameLanguage.TR &&
            subtitleFont != null)
        {
            subtitleLabel.font = subtitleFont;
        }
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        var canvasObject = new GameObject("LevelIntroCanvas");
        canvasObject.transform.SetParent(transform, false);

        overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = overlaySortOrder;
        overlayCanvas.pixelPerfect = false;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>().enabled = false;

        var rootObject = new GameObject("IntroRoot", typeof(RectTransform));
        rootObject.transform.SetParent(canvasObject.transform, false);
        var rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        rootGroup = rootObject.AddComponent<CanvasGroup>();
        rootGroup.alpha = 0f;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;

        contentRoot = CreateRect("Content", rootRect);
        contentRoot.anchorMin = new Vector2(0.5f, 0.5f);
        contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        contentRoot.pivot = new Vector2(0.5f, 0.5f);
        contentRoot.anchoredPosition = new Vector2(0f, 28f);
        contentRoot.sizeDelta = new Vector2(920f, 260f);

        backdrop = CreateImage("Backdrop", contentRoot);
        var backdropRect = backdrop.rectTransform;
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = new Vector2(-36f, -28f);
        backdropRect.offsetMax = new Vector2(36f, 28f);
        backdrop.color = new Color(0.02f, 0.04f, 0.09f, 0.58f);
        backdrop.raycastTarget = false;

        subtitleLabel = CreateText("Subtitle", contentRoot, 34f, new Color(0.62f, 0.82f, 0.96f, 1f), FontStyles.Normal);
        var subtitleRect = subtitleLabel.rectTransform;
        subtitleRect.anchorMin = new Vector2(0.5f, 1f);
        subtitleRect.anchorMax = new Vector2(0.5f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0f, -8f);
        subtitleRect.sizeDelta = new Vector2(760f, 48f);
        subtitleLabel.alignment = TextAlignmentOptions.Center;
        subtitleLabel.characterSpacing = 8f;

        CreateAccentLine(contentRoot, -132f);
        CreateAccentLine(contentRoot, 132f);

        titleShadowLabel = CreateText("TitleShadow", contentRoot, 92f, new Color(0f, 0f, 0f, 0.35f), FontStyles.Normal);
        var shadowRect = titleShadowLabel.rectTransform;
        shadowRect.anchorMin = new Vector2(0.5f, 0f);
        shadowRect.anchorMax = new Vector2(0.5f, 0f);
        shadowRect.pivot = new Vector2(0.5f, 0f);
        shadowRect.anchoredPosition = new Vector2(3f, 18f);
        shadowRect.sizeDelta = new Vector2(860f, 130f);
        titleShadowLabel.alignment = TextAlignmentOptions.Center;

        titleLabel = CreateText("Title", contentRoot, 92f, Color.white, FontStyles.Normal);
        var titleRect = titleLabel.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 0f);
        titleRect.anchorMax = new Vector2(0.5f, 0f);
        titleRect.pivot = new Vector2(0.5f, 0f);
        titleRect.anchoredPosition = new Vector2(0f, 22f);
        titleRect.sizeDelta = new Vector2(860f, 130f);
        titleLabel.alignment = TextAlignmentOptions.Center;
    }

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        var rectObject = new GameObject(objectName, typeof(RectTransform));
        rectObject.transform.SetParent(parent, false);
        return rectObject.GetComponent<RectTransform>();
    }

    private static Image CreateImage(string objectName, Transform parent)
    {
        var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        var image = imageObject.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        float fontSize,
        Color color,
        FontStyles fontStyle)
    {
        var textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        var label = textObject.GetComponent<TextMeshProUGUI>();
        label.fontSize = fontSize;
        label.color = color;
        label.fontStyle = fontStyle;
        label.raycastTarget = false;
        return label;
    }

    private static void CreateAccentLine(Transform parent, float xOffset)
    {
        var line = CreateImage("AccentLine", parent);
        var rect = line.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(xOffset, -34f);
        rect.sizeDelta = new Vector2(120f, 3f);
        line.color = new Color(0.45f, 0.78f, 0.95f, 0.85f);
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private static float EaseInCubic(float t)
    {
        return t * t * t;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
