using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class GameUI : MonoBehaviour
{
    [Header("Yildiz Gorselleri")]
    [SerializeField] private Sprite emptyStarSprite;
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private float starSize = 48f;
    [SerializeField] private float starSpacing = 10f;

    private Image[] starImages;

    void Awake()
    {
        LoadSpritesIfNeeded();
    }

    void Start()
    {
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
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnStarsChanged -= HandleStarsChanged;
        GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
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
        Transform legacyStarText = transform.Find("StarText");
        if (existingDisplay != null)
        {
            Destroy(existingDisplay.gameObject);
        }

        if (legacyStarText != null)
        {
            legacyStarText.gameObject.SetActive(false);
        }

        int starCount = GameManager.Instance != null ? GameManager.Instance.MaxStarsPerLevel : 3;

        GameObject container = new GameObject("StarDisplay", typeof(RectTransform));
        container.transform.SetParent(transform, false);

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

            RectTransform starRect = starObject.GetComponent<RectTransform>();
            starRect.sizeDelta = new Vector2(starSize, starSize);

            Image image = starObject.GetComponent<Image>();
            image.sprite = emptyStarSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            starImages[i] = image;
        }
    }

    private void HandleStarsChanged(int collected, int max)
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

            starImages[i].sprite = i < collected ? filledStarSprite : emptyStarSprite;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        // Menu, pause, level complete ekranlari burada yonetilebilir.
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
