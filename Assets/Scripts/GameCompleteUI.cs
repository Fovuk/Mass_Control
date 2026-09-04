using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(110)]
public class GameCompleteUI : MonoBehaviour
{
    private const int ConfettiCount = 72;
    private const int OverlaySortOrder = 5200;

    [SerializeField] private float fadeInDuration = 0.7f;
    [SerializeField] private float holdDuration = 3.4f;
    [SerializeField] private float fadeOutDuration = 0.85f;
    [SerializeField] private float confettiDuration = 4.2f;

    private Canvas overlayCanvas;
    private CanvasGroup rootGroup;
    private RectTransform contentRoot;
    private RectTransform confettiRoot;
    private Image dimmer;
    private TextMeshProUGUI titleLabel;
    private TextMeshProUGUI titleShadowLabel;
    private TextMeshProUGUI messageLabel;
    private TextMeshProUGUI returningLabel;
    private ConfettiPiece[] confettiPieces;
    private Coroutine celebrationRoutine;
    private bool isShowing;

    private static readonly Color[] ConfettiColors =
    {
        new Color(1f, 0.32f, 0.38f, 1f),
        new Color(1f, 0.72f, 0.22f, 1f),
        new Color(0.35f, 0.86f, 0.48f, 1f),
        new Color(0.35f, 0.72f, 1f, 1f),
        new Color(0.78f, 0.48f, 1f, 1f),
        new Color(1f, 0.55f, 0.78f, 1f),
        new Color(1f, 0.95f, 0.35f, 1f),
    };

    private struct ConfettiPiece
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 Velocity;
        public float RotationSpeed;
        public float Life;
        public float MaxLife;
        public Vector2 Size;
    }

    public static void EnsureOn(GameManager manager)
    {
        if (manager == null || manager.GetComponent<GameCompleteUI>() != null)
        {
            return;
        }

        manager.gameObject.AddComponent<GameCompleteUI>();
    }

    void Start()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(GameManager.Instance.CurrentState);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.LevelComplete &&
            GameManager.Instance != null &&
            GameManager.Instance.IsFinalLevel())
        {
            BeginCelebration();
            return;
        }

        if (isShowing && state != GameState.LevelComplete)
        {
            StopCelebrationVisuals();
        }
    }

    private void BeginCelebration()
    {
        EnsureOverlay();
        ApplyContent();
        ResetConfetti();

        overlayCanvas.gameObject.SetActive(true);
        isShowing = true;

        if (celebrationRoutine != null)
        {
            StopCoroutine(celebrationRoutine);
        }

        celebrationRoutine = StartCoroutine(PlayCelebration());
    }

    private IEnumerator PlayCelebration()
    {
        SetVisualState(0f, 36f, 0.92f);
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            float eased = EaseOutCubic(t);
            SetVisualState(eased, Mathf.Lerp(36f, 0f, eased), Mathf.Lerp(0.92f, 1f, EaseOutBack(t)));
            StepConfetti(Time.unscaledDeltaTime);
            yield return null;
        }

        SetVisualState(1f, 0f, 1f);

        float holdElapsed = 0f;
        while (holdElapsed < holdDuration)
        {
            holdElapsed += Time.unscaledDeltaTime;
            float pulse = 1f + Mathf.Sin(holdElapsed * 3.6f) * 0.015f;
            contentRoot.localScale = Vector3.one * pulse;
            StepConfetti(Time.unscaledDeltaTime);
            yield return null;
        }

        contentRoot.localScale = Vector3.one;
        elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            float eased = EaseInCubic(t);
            SetVisualState(1f - eased, Mathf.Lerp(0f, -18f, eased), Mathf.Lerp(1f, 0.96f, eased));
            StepConfetti(Time.unscaledDeltaTime);
            yield return null;
        }

        celebrationRoutine = null;
        isShowing = false;
        GameManager.Instance?.LoadMainMenu();
    }

    private void StopCelebrationVisuals()
    {
        if (celebrationRoutine != null)
        {
            StopCoroutine(celebrationRoutine);
            celebrationRoutine = null;
        }

        isShowing = false;

        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(false);
        }
    }

    private void SetVisualState(float alpha, float offsetY, float scale)
    {
        if (rootGroup != null)
        {
            rootGroup.alpha = alpha;
        }

        if (dimmer != null)
        {
            Color dimmerColor = dimmer.color;
            dimmerColor.a = 0.62f * alpha;
            dimmer.color = dimmerColor;
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

        string title = LocalizationManager.Get("game_complete_title");
        string message = LocalizationManager.Get("game_complete_message");
        string returning = LocalizationManager.Get("game_complete_returning");

        if (LocalizationManager.Instance != null &&
            LocalizationManager.Instance.CurrentLanguage == GameLanguage.TR)
        {
            title = ToFontSafeAscii(title);
            message = ToFontSafeAscii(message);
            returning = ToFontSafeAscii(returning);
        }

        ApplyFonts();
        titleLabel.text = title;
        titleShadowLabel.text = title;
        messageLabel.text = message;
        returningLabel.text = returning;
    }

    private void ApplyFonts()
    {
        LevelIntroSettings settings = LevelIntroSettings.Get();
        TMP_FontAsset fallback = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        TMP_FontAsset titleFont = settings != null && settings.titleFont != null
            ? settings.titleFont
            : fallback;
        TMP_FontAsset bodyFont = settings != null && settings.subtitleFont != null
            ? settings.subtitleFont
            : fallback;

        if (titleFont != null)
        {
            titleLabel.font = titleFont;
            titleShadowLabel.font = titleFont;
        }

        if (bodyFont != null)
        {
            messageLabel.font = bodyFont;
            returningLabel.font = bodyFont;
        }
        else
        {
            LocalizationManager.ApplyUiFont(messageLabel);
            LocalizationManager.ApplyUiFont(returningLabel);
        }
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        var canvasObject = new GameObject("GameCompleteCanvas");
        canvasObject.transform.SetParent(transform, false);

        overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = OverlaySortOrder;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>().enabled = false;

        var rootObject = new GameObject("CompleteRoot", typeof(RectTransform));
        rootObject.transform.SetParent(canvasObject.transform, false);
        var rootRect = rootObject.GetComponent<RectTransform>();
        StretchFull(rootRect);

        rootGroup = rootObject.AddComponent<CanvasGroup>();
        rootGroup.alpha = 0f;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;

        dimmer = CreateImage("Dimmer", rootRect);
        StretchFull(dimmer.rectTransform);
        dimmer.color = new Color(0.04f, 0.05f, 0.1f, 0.62f);

        confettiRoot = CreateRect("Confetti", rootRect);
        StretchFull(confettiRoot);

        contentRoot = CreateRect("Content", rootRect);
        contentRoot.anchorMin = new Vector2(0.5f, 0.5f);
        contentRoot.anchorMax = new Vector2(0.5f, 0.5f);
        contentRoot.pivot = new Vector2(0.5f, 0.5f);
        contentRoot.sizeDelta = new Vector2(980f, 320f);

        var card = CreateImage("Card", contentRoot);
        StretchFull(card.rectTransform);
        card.rectTransform.offsetMin = new Vector2(-40f, -36f);
        card.rectTransform.offsetMax = new Vector2(40f, 36f);
        card.color = new Color(0.03f, 0.05f, 0.1f, 0.72f);

        titleShadowLabel = CreateText("TitleShadow", contentRoot, 96f, new Color(0f, 0f, 0f, 0.35f));
        PlaceCentered(titleShadowLabel.rectTransform, new Vector2(4f, 78f), new Vector2(900f, 130f));

        titleLabel = CreateText("Title", contentRoot, 96f, new Color(1f, 0.92f, 0.55f, 1f));
        PlaceCentered(titleLabel.rectTransform, new Vector2(0f, 82f), new Vector2(900f, 130f));

        messageLabel = CreateText("Message", contentRoot, 42f, Color.white);
        PlaceCentered(messageLabel.rectTransform, new Vector2(0f, -8f), new Vector2(860f, 70f));

        returningLabel = CreateText("Returning", contentRoot, 28f, new Color(0.72f, 0.8f, 0.9f, 0.9f));
        PlaceCentered(returningLabel.rectTransform, new Vector2(0f, -78f), new Vector2(760f, 48f));

        BuildConfettiPool();
        overlayCanvas.gameObject.SetActive(false);
    }

    private void BuildConfettiPool()
    {
        confettiPieces = new ConfettiPiece[ConfettiCount];

        for (int i = 0; i < ConfettiCount; i++)
        {
            Image piece = CreateImage($"Confetti_{i}", confettiRoot);
            piece.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            piece.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            piece.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            piece.gameObject.SetActive(false);

            confettiPieces[i] = new ConfettiPiece
            {
                Rect = piece.rectTransform,
                Image = piece,
            };
        }
    }

    private void ResetConfetti()
    {
        if (confettiPieces == null)
        {
            return;
        }

        for (int i = 0; i < confettiPieces.Length; i++)
        {
            SpawnConfetti(ref confettiPieces[i], Random.Range(0f, 0.55f));
        }
    }

    private void SpawnConfetti(ref ConfettiPiece piece, float delay)
    {
        float width = Random.Range(10f, 22f);
        float height = Random.Range(16f, 34f);
        piece.Size = new Vector2(width, height);
        piece.Rect.sizeDelta = piece.Size;
        piece.Rect.anchoredPosition = new Vector2(Random.Range(-960f, 960f), Random.Range(40f, 220f));
        piece.Rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        piece.Velocity = new Vector2(Random.Range(-180f, 180f), Random.Range(-420f, -180f));
        piece.RotationSpeed = Random.Range(-420f, 420f);
        piece.MaxLife = confettiDuration;
        piece.Life = -delay;
        piece.Image.color = ConfettiColors[Random.Range(0, ConfettiColors.Length)];
        piece.Image.gameObject.SetActive(true);
    }

    private void StepConfetti(float deltaTime)
    {
        if (confettiPieces == null)
        {
            return;
        }

        for (int i = 0; i < confettiPieces.Length; i++)
        {
            ConfettiPiece piece = confettiPieces[i];
            piece.Life += deltaTime;

            if (piece.Life < 0f)
            {
                confettiPieces[i] = piece;
                continue;
            }

            if (piece.Life > piece.MaxLife || piece.Rect.anchoredPosition.y < -1200f)
            {
                SpawnConfetti(ref piece, Random.Range(0f, 0.2f));
                confettiPieces[i] = piece;
                continue;
            }

            piece.Velocity.y -= 520f * deltaTime;
            piece.Velocity.x += Mathf.Sin((piece.Life + i) * 6.5f) * 40f * deltaTime;
            piece.Rect.anchoredPosition += piece.Velocity * deltaTime;
            piece.Rect.Rotate(0f, 0f, piece.RotationSpeed * deltaTime);

            float fade = 1f - Mathf.Clamp01((piece.Life - piece.MaxLife + 0.8f) / 0.8f);
            Color color = piece.Image.color;
            color.a = fade;
            piece.Image.color = color;

            confettiPieces[i] = piece;
        }
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void PlaceCentered(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
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

    private static TextMeshProUGUI CreateText(string objectName, Transform parent, float fontSize, Color color)
    {
        var textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        var label = textObject.GetComponent<TextMeshProUGUI>();
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private static string ToFontSafeAscii(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        foreach (char character in text)
        {
            builder.Append(character switch
            {
                'İ' => 'I',
                'ı' => 'i',
                'Ğ' => 'G',
                'ğ' => 'g',
                'Ü' => 'U',
                'ü' => 'u',
                'Ş' => 'S',
                'ş' => 's',
                'Ö' => 'O',
                'ö' => 'o',
                'Ç' => 'C',
                'ç' => 'c',
                _ => character
            });
        }

        return builder.ToString();
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
