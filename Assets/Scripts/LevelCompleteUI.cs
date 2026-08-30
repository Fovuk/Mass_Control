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

        EnsureStarSpritesAssigned();
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
            UpdateStarDisplay();
            UpdateSummaryText();
            UpdateNextLevelButton();
        }
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
        UpdateStarDisplay();
        UpdateSummaryText();
        UpdateNextLevelButton();
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
            LocalizationManager.ApplyUiFont(nextLevelHintText);
        }

        bool showUnlockRequirement = GameManager.Instance.HasNextLevel() &&
            !GameManager.Instance.IsNextLevelUnlocked();

        if (showUnlockRequirement)
        {
            summaryText.gameObject.SetActive(true);
            summaryText.rectTransform.anchoredPosition = new Vector2(0f, 30f);
            summaryText.rectTransform.sizeDelta = new Vector2(640f, 60f);
            summaryText.alignment = TextAlignmentOptions.Center;
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

    private void UpdateNextLevelButton()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        bool canLoadNextLevel = GameManager.Instance.CanLoadNextLevel();

        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = canLoadNextLevel;
        }

        if (nextLevelHintText != null)
        {
            if (!GameManager.Instance.HasNextLevel())
            {
                nextLevelHintText.text = string.Empty;
            }
            else if (canLoadNextLevel)
            {
                nextLevelHintText.text = string.Empty;
            }
            else
            {
                nextLevelHintText.text = LocalizationManager.Format(
                    "unlock_hint",
                    SaveManager.StarsRequiredToUnlockNextLevel);
            }
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
