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
    }

    void Start()
    {
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        playAgainButton?.onClick.RemoveListener(OnPlayAgainClicked);
        nextLevelButton?.onClick.RemoveListener(OnNextLevelClicked);
        mainMenuButton?.onClick.RemoveListener(OnMainMenuClicked);
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
        UpdateSummaryText();
        UpdateNextLevelButton();
        Debug.Log("[LevelCompleteUI] Panel gosterildi.");
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

    private void UpdateSummaryText()
    {
        if (summaryText == null || GameManager.Instance == null)
        {
            return;
        }

        int collected = GameManager.Instance.CollectedStars;
        int max = GameManager.Instance.MaxStarsPerLevel;
        string summary = "Bölüm Tamamlandı!\nYıldız Sayısı: " + collected + "/" + max;

        if (GameManager.Instance.HasNextLevel() && !GameManager.Instance.IsNextLevelUnlocked())
        {
            summary += "\n\nSonraki bölüm için " + SaveManager.StarsRequiredToUnlockNextLevel + " yıldız gerekli.";
        }

        summaryText.text = summary;
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
                nextLevelHintText.text = SaveManager.StarsRequiredToUnlockNextLevel + " yıldız ile açılır";
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
