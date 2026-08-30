#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class LevelSelectController : MonoBehaviour
{
    [System.Serializable]
    private class LevelButtonEntry
    {
        public Button button;
        public int sceneBuildIndex = 1;
    }

    [Header("Bölüm Butonları")]
    [SerializeField] private LevelButtonEntry[] levelButtons;

    [Header("Kilit Ayarları")]
    [SerializeField] private int starsRequiredToUnlock = SaveManager.StarsRequiredToUnlockNextLevel;

    void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        EnsureSaveManager();
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }
#endif

        EnsureStarDisplays();
        RefreshLevelButtons();

        if (!Application.isPlaying)
        {
            return;
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnProgressSaved += HandleProgressSaved;
        }

        BindButtonListeners();
    }

    void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        RefreshLevelButtons();
    }

    void OnDisable()
    {
        if (!Application.isPlaying)
        {
            return;
        }

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

    private void EnsureStarDisplays()
    {
        if (levelButtons == null)
        {
            return;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            Button button = levelButtons[i]?.button;
            if (button == null)
            {
                continue;
            }

            if (button.GetComponent<LevelStarDisplay>() == null)
            {
                button.gameObject.AddComponent<LevelStarDisplay>();
            }
        }
    }

    private void RefreshLevelButtons()
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

            LevelStarDisplay starDisplay = entry.button.GetComponent<LevelStarDisplay>();
            if (starDisplay == null)
            {
                continue;
            }

            if (!Application.isPlaying)
            {
                starDisplay.SetStars(0);
                continue;
            }

            if (SaveManager.Instance == null)
            {
                continue;
            }

            int sceneIndex = entry.sceneBuildIndex;
            bool isUnlocked = SaveManager.Instance.IsLevelUnlocked(sceneIndex, starsRequiredToUnlock);
            entry.button.interactable = isUnlocked;

            int stars = isUnlocked ? SaveManager.Instance.GetLevelStars(sceneIndex) : 0;
            starDisplay.SetStars(stars);
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
