using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class LevelCompleteUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Metin")]
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private GameObject titleObject;

    [Header("Yildizlar")]
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite emptyStarSprite;
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Color filledStarColor = Color.white;
    [SerializeField] private Color emptyStarColor = new Color(0.62f, 0.64f, 0.68f, 1f);

    [Header("Butonlar")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI nextLevelHintText;

    private CanvasGroup panelCanvasGroup;
    private bool hideWithCanvasGroup;
    private Coroutine languageRefreshRoutine;

    private static readonly Color UnlockRequirementColor = new Color(0.42f, 0.46f, 0.52f, 1f);

    void Awake()
    {
        if (panel == null)
        {
            panel = gameObject;
        }

        hideWithCanvasGroup = panel == gameObject;
        if (hideWithCanvasGroup)
        {
            panelCanvasGroup = panel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = panel.AddComponent<CanvasGroup>();
            }
        }

        playAgainButton?.onClick.AddListener(OnPlayAgainClicked);
        nextLevelButton?.onClick.AddListener(OnNextLevelClicked);
        mainMenuButton?.onClick.AddListener(OnMainMenuClicked);

        EnsureTitleReference();
        EnsureStarSpritesAssigned();
    }

    private void EnsureTitleReference()
    {
        if (titleObject != null || panel == null)
        {
            return;
        }

        Transform completeCard = panel.transform.Find("CompleteCard");
        if (completeCard == null)
        {
            completeCard = panel.transform;
        }

        Transform title = completeCard.Find("Title");
        if (title != null)
        {
            titleObject = title.gameObject;
        }
    }

    void Start()
    {
        LocalizationManager.EnsureInstance();
        LocalizationManager.Instance.OnLanguageChanged += HandleLanguageChanged;

        if (GameManager.Instance == null)
        {
            Debug.LogError("[LevelCompleteUI] GameManager bulunamadi.");
            return;
        }

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(GameManager.Instance.CurrentState);
    }

    void OnDestroy()
    {
        if (languageRefreshRoutine != null)
        {
            StopCoroutine(languageRefreshRoutine);
            languageRefreshRoutine = null;
        }

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        playAgainButton?.onClick.RemoveListener(OnPlayAgainClicked);
        nextLevelButton?.onClick.RemoveListener(OnNextLevelClicked);
        mainMenuButton?.onClick.RemoveListener(OnMainMenuClicked);
    }

    private void HandleLanguageChanged()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.LevelComplete)
        {
            if (languageRefreshRoutine != null)
            {
                StopCoroutine(languageRefreshRoutine);
            }

            languageRefreshRoutine = StartCoroutine(RefreshAfterLanguageChange());
        }
    }

    private IEnumerator RefreshAfterLanguageChange()
    {
        yield return null;
        RefreshPanelContent();
        languageRefreshRoutine = null;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.LevelComplete)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
        }
    }

    private void ShowPanel()
    {
        SetPanelVisible(true);
        RefreshPanelContent();
    }

    private void RefreshPanelContent()
    {
        UpdateTitleVisibility();
        UpdateStarDisplay();
        UpdateSummaryText();
        UpdateNextLevelButton();
    }

    private bool EarnedEnoughStarsThisRun()
    {
        if (GameManager.Instance == null)
        {
            return false;
        }

        return GameManager.Instance.CollectedStars >= GameManager.Instance.MaxStarsPerLevel;
    }

    private bool ShouldShowNextLevelButton()
    {
        return GameManager.Instance != null &&
            GameManager.Instance.HasNextLevel() &&
            EarnedEnoughStarsThisRun();
    }

    private bool ShouldShowUnlockRequirement()
    {
        return GameManager.Instance != null &&
            GameManager.Instance.HasNextLevel() &&
            !EarnedEnoughStarsThisRun();
    }

    private void HidePanel()
    {
        SetPanelVisible(false);
    }

    private void SetPanelVisible(bool visible)
    {
        if (hideWithCanvasGroup)
        {
            panelCanvasGroup.alpha = visible ? 1f : 0f;
            panelCanvasGroup.blocksRaycasts = visible;
            panelCanvasGroup.interactable = visible;
            return;
        }

        panel.SetActive(visible);
    }

    private void EnsureStarSpritesAssigned()
    {
        if (emptyStarSprite != null && filledStarSprite != null)
        {
            return;
        }

        emptyStarSprite ??= StarUiSprites.GetEmptyStar();
        filledStarSprite ??= StarUiSprites.GetFilledStar();
    }

    private void UpdateStarDisplay()
    {
        if (starImages == null || starImages.Length == 0 || GameManager.Instance == null)
        {
            return;
        }

        EnsureStarSpritesAssigned();

        if (emptyStarSprite == null || filledStarSprite == null)
        {
            return;
        }

        int collected = GameManager.Instance.CollectedStars;

        for (int i = 0; i < starImages.Length; i++)
        {
            Image starImage = starImages[i];
            if (starImage == null)
            {
                continue;
            }

            bool isFilled = i < collected;
            starImage.sprite = isFilled ? filledStarSprite : emptyStarSprite;
            starImage.color = isFilled ? filledStarColor : emptyStarColor;
        }
    }

    private void UpdateTitleVisibility()
    {
        if (titleObject != null)
        {
            titleObject.SetActive(false);
        }
    }

    private void UpdateSummaryText()
    {
        if (summaryText == null || GameManager.Instance == null)
        {
            return;
        }

        if (LocalizationManager.Instance != null &&
            LocalizationManager.Instance.CurrentLanguage == GameLanguage.TR)
        {
            LocalizationManager.ApplyUiFont(summaryText);
        }

        if (ShouldShowUnlockRequirement())
        {
            summaryText.gameObject.SetActive(true);
            summaryText.rectTransform.anchoredPosition = GetNextLevelSlotPosition();
            summaryText.rectTransform.sizeDelta = new Vector2(640f, 84f);
            summaryText.alignment = TextAlignmentOptions.Center;
            summaryText.fontSize = 28f;
            summaryText.fontStyle = FontStyles.Italic;
            summaryText.color = UnlockRequirementColor;
            summaryText.text = LocalizationManager.Format(
                "unlock_requirement",
                SaveManager.StarsRequiredToUnlockNextLevel);
        }
        else
        {
            summaryText.text = string.Empty;
            summaryText.gameObject.SetActive(false);
        }
    }

    private Vector2 GetNextLevelSlotPosition()
    {
        if (nextLevelButton != null &&
            nextLevelButton.TryGetComponent(out RectTransform nextLevelRect))
        {
            return nextLevelRect.anchoredPosition;
        }

        return new Vector2(0f, -20f);
    }

    private void UpdateNextLevelButton()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        bool showNextLevelButton = ShouldShowNextLevelButton();

        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(showNextLevelButton);
            nextLevelButton.interactable = showNextLevelButton;
        }

        if (nextLevelHintText != null)
        {
            nextLevelHintText.text = string.Empty;
        }
    }

    public void OnPlayAgainClicked()
    {
        GameManager.Instance?.RestartCurrentLevel();
    }

    public void OnNextLevelClicked()
    {
        GameManager.Instance?.LoadNextLevel();
    }

    public void OnMainMenuClicked()
    {
        GameManager.Instance?.LoadMainMenu();
    }
}
