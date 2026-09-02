using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum GameLanguage
{
    TR,
    EN
}

[DefaultExecutionOrder(-150)]
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private const string LanguageKey = "Settings_Language";

    private static readonly Dictionary<string, string[]> Translations = new Dictionary<string, string[]>
    {
        { "game_title", new[] { "MASS CONTROL", "MASS CONTROL" } },
        { "settings", new[] { "Ayarlar", "Settings" } },
        { "language", new[] { "Dil", "Language" } },
        { "music", new[] { "Müzik", "Music" } },
        { "sfx", new[] { "SFX", "SFX" } },
        { "level_1", new[] { "Bölüm 1", "Level 1" } },
        { "level_2", new[] { "Bölüm 2", "Level 2" } },
        { "level_3", new[] { "Bölüm 3", "Level 3" } },
        { "level_4", new[] { "Bölüm 4", "Level 4" } },
        { "level_5", new[] { "Bölüm 5", "Level 5" } },
        { "level_6", new[] { "Bölüm 6", "Level 6" } },
        { "level_intro_format", new[] { "BÖLÜM {0}", "LEVEL {0}" } },
        { "level_name_1", new[] { "İlk Kütle", "First Shift" } },
        { "level_name_2", new[] { "Uzun Atlama", "Long Jump" } },
        { "level_name_3", new[] { "Dikenler", "Spikes" } },
        { "level_name_4", new[] { "Zıpla!", "Jump!" } },
        { "level_name_5", new[] { "Uçurum", "Cliff" } },
        { "level_name_6", new[] { "Final", "Final" } },
        { "level_complete", new[] { "Bölüm Tamamlandı!", "Level Complete!" } },
        { "stars_format", new[] { "Yıldız: {0}/{1}", "Stars: {0}/{1}" } },
        { "unlock_requirement", new[] { "Sonraki bölüm için {0} yıldız gerekli.", "Collect all {0} stars to continue." } },
        { "unlock_hint", new[] { "{0} yıldız ile açılır", "Unlocks with {0} stars" } },
        { "next_level", new[] { "Sıradaki Bölüm", "Next Level" } },
        { "main_menu", new[] { "Ana Menü", "Main Menu" } },
        { "play_again", new[] { "Tekrar Oyna", "Play Again" } },
        { "morph", new[] { "Boyut\nDeğiştir", "Change\nSize" } },
        { "jump", new[] { "Zıplama", "Jump" } },
        { "quit_game", new[] { "Çıkış", "Quit" } },
        { "quit_confirm_title", new[] { "Emin misin?", "Are you sure?" } },
        { "quit_confirm_message", new[] { "Oyun kapatılacak.", "The game will close." } },
        { "yes", new[] { "Evet", "Yes" } },
        { "no", new[] { "Hayır", "No" } },
    };

    public GameLanguage CurrentLanguage { get; private set; } = GameLanguage.TR;

    public event Action OnLanguageChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static LocalizationManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var existing = FindAnyObjectByType<LocalizationManager>();
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        var managerObject = new GameObject("LocalizationManager");
        Instance = managerObject.AddComponent<LocalizationManager>();
        DontDestroyOnLoad(managerObject);
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadLanguage();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetLanguage(GameLanguage language)
    {
        if (CurrentLanguage == language)
        {
            return;
        }

        CurrentLanguage = language;
        PlayerPrefs.SetInt(LanguageKey, (int)language);
        PlayerPrefs.Save();
        OnLanguageChanged?.Invoke();
    }

    public static string Get(string key)
    {
        EnsureInstance();

        if (string.IsNullOrEmpty(key) || !Translations.TryGetValue(key, out string[] values))
        {
            return key ?? string.Empty;
        }

        int index = Instance != null ? (int)Instance.CurrentLanguage : 0;
        index = Mathf.Clamp(index, 0, values.Length - 1);
        return values[index];
    }

    public static string Format(string key, params object[] args)
    {
        return string.Format(Get(key), args);
    }

    public static void ApplyUiFont(TextMeshProUGUI label)
    {
        if (label == null)
        {
            return;
        }

        TMP_FontAsset uiFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (uiFont != null)
        {
            label.font = uiFont;
        }
    }

    private void LoadLanguage()
    {
        int savedLanguage = PlayerPrefs.GetInt(LanguageKey, (int)GameLanguage.TR);
        CurrentLanguage = Enum.IsDefined(typeof(GameLanguage), savedLanguage)
            ? (GameLanguage)savedLanguage
            : GameLanguage.TR;
    }
}
