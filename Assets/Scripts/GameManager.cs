using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Bölüm Ayarları")]
    [SerializeField] private int maxStarsPerLevel = 3;

    [Header("Sahne Ayarları")]
    [SerializeField] private int mainMenuSceneIndex = 0;

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public int CollectedStars { get; private set; }
    public int MaxStarsPerLevel => maxStarsPerLevel;

    public event Action<GameState> OnGameStateChanged;
    public event Action<int, int> OnStarsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        SetState(GameState.Playing);
        NotifyStarsChanged();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
        {
            Debug.Log($"[GameManager] Durum zaten {newState}, degisiklik yok.");
            return;
        }

        Debug.Log($"[GameManager] Durum degisti: {CurrentState} -> {newState}");
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    public void CollectStar(GameObject star)
    {
        if (CurrentState != GameState.Playing || CollectedStars >= maxStarsPerLevel)
        {
            return;
        }

        CollectedStars++;
        Destroy(star);
        NotifyStarsChanged();
    }

    public void PlayerDied()
    {
        if (CurrentState == GameState.Dead)
        {
            return;
        }

        SetState(GameState.Dead);
        DieAndRestart();
    }

    public void CompleteLevel()
    {
        if (CurrentState == GameState.LevelComplete)
        {
            return;
        }

        if (CurrentState != GameState.Playing)
        {
            Debug.LogWarning($"[GameManager] Bolum tamamlanamadi, oyun durumu uygun degil: {CurrentState}");
            return;
        }

        SetState(GameState.LevelComplete);
        Time.timeScale = 0f;

        int levelIndex = SceneManager.GetActiveScene().buildIndex;
        SaveManager.Instance?.SaveLevelProgress(levelIndex, CollectedStars);

        Debug.Log("[GameManager] Bolum tamamlandi!");
    }

    public bool HasNextLevel()
    {
        return SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        if (!HasNextLevel())
        {
            Debug.LogWarning("[GameManager] Yuklenecek sonraki bolum yok.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    public void DieAndRestart()
    {
        Time.timeScale = 1f;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void NotifyStarsChanged()
    {
        OnStarsChanged?.Invoke(CollectedStars, maxStarsPerLevel);
    }
}
