using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-200)]
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const int StarsRequiredToUnlockNextLevel = 3;

    private const string HighestUnlockedLevelKey = "HighestUnlockedLevel";
    private const string LevelStarsKeyPrefix = "LevelStars_";

    public event Action<int> OnProgressSaved;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureFirstLevelUnlocked();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public int GetLevelStars(int levelIndex)
    {
        if (levelIndex < 0)
        {
            return 0;
        }

        return PlayerPrefs.GetInt(GetStarsKey(levelIndex), 0);
    }

    public int GetHighestUnlockedLevel()
    {
        return PlayerPrefs.GetInt(HighestUnlockedLevelKey, 0);
    }

    public bool IsLevelUnlocked(int levelIndex, int starsRequired = StarsRequiredToUnlockNextLevel)
    {
        if (levelIndex <= 1)
        {
            return true;
        }

        return GetLevelStars(levelIndex - 1) >= starsRequired;
    }

    public void SaveLevelProgress(int levelIndex, int starsCollected)
    {
        if (levelIndex < 0)
        {
            return;
        }

        starsCollected = Mathf.Max(0, starsCollected);
        bool changed = false;

        int bestStars = GetLevelStars(levelIndex);
        if (starsCollected > bestStars)
        {
            PlayerPrefs.SetInt(GetStarsKey(levelIndex), starsCollected);
            changed = true;
        }

        int nextLevelIndex = levelIndex + 1;
        if (starsCollected >= StarsRequiredToUnlockNextLevel && nextLevelIndex > GetHighestUnlockedLevel())
        {
            PlayerPrefs.SetInt(HighestUnlockedLevelKey, nextLevelIndex);
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        PlayerPrefs.Save();
        OnProgressSaved?.Invoke(levelIndex);
    }

    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(HighestUnlockedLevelKey);

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            PlayerPrefs.DeleteKey(GetStarsKey(i));
        }

        PlayerPrefs.Save();
        EnsureFirstLevelUnlocked();
        OnProgressSaved?.Invoke(-1);
    }

    private void EnsureFirstLevelUnlocked()
    {
        if (!PlayerPrefs.HasKey(HighestUnlockedLevelKey))
        {
            PlayerPrefs.SetInt(HighestUnlockedLevelKey, 0);
            PlayerPrefs.Save();
        }
    }

    private static string GetStarsKey(int levelIndex)
    {
        return LevelStarsKeyPrefix + levelIndex;
    }
}
