using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    [System.Serializable]
    private class LevelButtonEntry
    {
        public Button button;
        public int sceneBuildIndex = 1;
        public TextMeshProUGUI starCountText;
    }

    [Header("Bölüm Butonları")]
    [SerializeField] private LevelButtonEntry[] levelButtons;

    [Header("Kilit Ayarları")]
    [SerializeField] private int starsRequiredToUnlock = SaveManager.StarsRequiredToUnlockNextLevel;

    void Awake()
    {
        EnsureSaveManager();
    }

    void OnEnable()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnProgressSaved += HandleProgressSaved;
        }

        BindButtonListeners();
        RefreshLevelButtons();
    }

    void OnDisable()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnProgressSaved -= HandleProgressSaved;
        }

        UnbindButtonListeners();
    }

    private void EnsureSaveManager()
    {
        if (SaveManager.Instance != null)
        {
            return;
        }

        var saveManagerObject = new GameObject("SaveManager");
        saveManagerObject.AddComponent<SaveManager>();
    }

    private void BindButtonListeners()
    {
        if (levelButtons == null)
        {
            return;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            LevelButtonEntry entry = levelButtons[i];
            if (entry?.button == null)
            {
                continue;
            }

            int sceneIndex = entry.sceneBuildIndex;
            entry.button.onClick.AddListener(() => OnLevelButtonClicked(sceneIndex));
        }
    }

    private void UnbindButtonListeners()
    {
        if (levelButtons == null)
        {
            return;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i]?.button?.onClick.RemoveAllListeners();
        }
    }

    private void HandleProgressSaved(int levelIndex)
    {
        RefreshLevelButtons();
    }

    private void RefreshLevelButtons()
    {
        if (levelButtons == null || SaveManager.Instance == null)
        {
            return;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            LevelButtonEntry entry = levelButtons[i];
            if (entry?.button == null)
            {
                continue;
            }

            int sceneIndex = entry.sceneBuildIndex;
            bool isUnlocked = SaveManager.Instance.IsLevelUnlocked(sceneIndex, starsRequiredToUnlock);
            entry.button.interactable = isUnlocked;

            if (entry.starCountText != null)
            {
                int stars = SaveManager.Instance.GetLevelStars(sceneIndex);
                entry.starCountText.text = isUnlocked ? stars + "/" + starsRequiredToUnlock : "Kilitli";
            }
        }
    }

    private void OnLevelButtonClicked(int sceneBuildIndex)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[LevelSelectController] SaveManager bulunamadi.");
            return;
        }

        if (!SaveManager.Instance.IsLevelUnlocked(sceneBuildIndex, starsRequiredToUnlock))
        {
            Debug.LogWarning("[LevelSelectController] Bolum kilitli: " + sceneBuildIndex);
            return;
        }

        if (sceneBuildIndex < 0 || sceneBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("[LevelSelectController] Gecersiz sahne indeksi: " + sceneBuildIndex);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneBuildIndex);
    }
}
