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
    [SerializeField] private float deathBeatDuration = 0.85f;

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
        EnsureSfxManager();
        DeathScreenEffect.EnsureOn(this);
        LevelWakeUpEffect.EnsureOn(this);
        LevelIntroTitle.EnsureOn(this);
        GameCompleteUI.EnsureOn(this);
    }

    private void EnsureSfxManager()
    {
        if (SfxManager.Instance == null)
        {
            var audioManager = new GameObject("Audio Manager");
            audioManager.AddComponent<SfxManager>();
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
        SetState(GameState.Playing);
        NotifyStarsChanged();
        ValidateLevelStarCount();
        StartSceneMusicIfNeeded();
    }

    private void StartSceneMusicIfNeeded()
    {
        if (SceneManager.GetActiveScene().buildIndex == mainMenuSceneIndex)
        {
            SfxManager.Instance?.PlayMainMenuMusic();
            return;
        }

        SfxManager.Instance?.PlayInGameMusic();
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
        SfxManager.Instance?.PlayStarCollect();
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
        SfxManager.Instance?.StopInGameMusic();

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayDeath(DieAndRestart, deathBeatDuration);
        }
        else
        {
            StartCoroutine(RestartAfterDelay(deathBeatDuration));
        }
    }

    private System.Collections.IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
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
        SfxManager.Instance?.StopInGameMusic();
        SfxManager.Instance?.PlayLevelComplete();
        Time.timeScale = 0f;

        int levelIndex = SceneManager.GetActiveScene().buildIndex;
        SaveManager.Instance?.SaveLevelProgress(levelIndex, CollectedStars);

        Debug.Log("[GameManager] Bolum tamamlandi!");
    }

    public bool HasNextLevel()
    {
        return SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
    }

    public bool IsFinalLevel()
    {
        return !HasNextLevel() && SceneManager.GetActiveScene().buildIndex > mainMenuSceneIndex;
    }

    public bool IsNextLevelUnlocked()
    {
        if (!HasNextLevel())
        {
            return false;
        }

        if (SaveManager.Instance == null)
        {
            return CollectedStars >= maxStarsPerLevel;
        }

        int nextLevelBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;
        return SaveManager.Instance.IsLevelUnlocked(nextLevelBuildIndex);
    }

    public bool CanLoadNextLevel()
    {
        return HasNextLevel() && IsNextLevelUnlocked();
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

        if (!IsNextLevelUnlocked())
        {
            Debug.LogWarning("[GameManager] Sonraki bolum kilitli. Once 3 yildiz toplamalisin.");
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
        LevelIntroTitle.RequestSkipOnNextLoad();
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void NotifyStarsChanged()
    {
        OnStarsChanged?.Invoke(CollectedStars, maxStarsPerLevel);
    }

    private void ValidateLevelStarCount()
    {
        int starCount = GameObject.FindGameObjectsWithTag("Star").Length;
        if (starCount != maxStarsPerLevel)
        {
            Debug.LogWarning($"[GameManager] Bu bolumde {starCount} yildiz var, {maxStarsPerLevel} olmali.");
        }
    }
}
