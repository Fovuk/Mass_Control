using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private static readonly Color SelectedLanguageColor = new Color(0.22f, 0.74f, 0.97f, 1f);
    private static readonly Color NormalLanguageColor = Color.white;

    [SerializeField] private PointerDrivenSlider musicSlider;
    [SerializeField] private PointerDrivenSlider sfxSlider;
    [SerializeField] private Button languageTrButton;
    [SerializeField] private Button languageEnButton;

    void Awake()
    {
        if (musicSlider == null)
        {
            musicSlider = transform.Find("SettingsPanel/MusicSlider")?.GetComponent<PointerDrivenSlider>();
        }

        if (sfxSlider == null)
        {
            sfxSlider = transform.Find("SettingsPanel/SfxSlider")?.GetComponent<PointerDrivenSlider>();
        }

        if (languageTrButton == null)
        {
            languageTrButton = transform.Find("SettingsPanel/LanguageTR")?.GetComponent<Button>();
        }

        if (languageEnButton == null)
        {
            languageEnButton = transform.Find("SettingsPanel/LanguageEN")?.GetComponent<Button>();
        }
    }

    void OnEnable()
    {
        LocalizationManager.EnsureInstance();

        BindSlider(musicSlider, GetMusicVolume(), OnMusicChanged);
        BindSlider(sfxSlider, GetSfxVolume(), OnSfxChanged);

        languageTrButton?.onClick.AddListener(OnTurkishSelected);
        languageEnButton?.onClick.AddListener(OnEnglishSelected);
        LocalizationManager.Instance.OnLanguageChanged += RefreshLanguageButtons;
        RefreshLanguageButtons();
    }

    void OnDisable()
    {
        if (musicSlider != null)
        {
            musicSlider.OnValueChanged -= OnMusicChanged;
        }

        if (sfxSlider != null)
        {
            sfxSlider.OnValueChanged -= OnSfxChanged;
        }

        languageTrButton?.onClick.RemoveListener(OnTurkishSelected);
        languageEnButton?.onClick.RemoveListener(OnEnglishSelected);

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= RefreshLanguageButtons;
        }

        PlayerPrefs.Save();
    }

    private void OnTurkishSelected()
    {
        LocalizationManager.Instance.SetLanguage(GameLanguage.TR);
        SfxManager.Instance?.PlayButtonClick();
    }

    private void OnEnglishSelected()
    {
        LocalizationManager.Instance.SetLanguage(GameLanguage.EN);
        SfxManager.Instance?.PlayButtonClick();
    }

    private void RefreshLanguageButtons()
    {
        if (LocalizationManager.Instance == null)
        {
            return;
        }

        SetLanguageButtonVisual(languageTrButton, LocalizationManager.Instance.CurrentLanguage == GameLanguage.TR);
        SetLanguageButtonVisual(languageEnButton, LocalizationManager.Instance.CurrentLanguage == GameLanguage.EN);
    }

    private static void SetLanguageButtonVisual(Button button, bool selected)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.targetGraphic as Image;
        if (image != null)
        {
            image.color = selected ? SelectedLanguageColor : NormalLanguageColor;
        }
    }

    private void BindSlider(PointerDrivenSlider slider, float currentValue, System.Action<float> callback)
    {
        if (slider == null)
        {
            return;
        }

        slider.OnValueChanged -= callback;
        slider.SetValueWithoutNotify(currentValue);
        slider.OnValueChanged += callback;
    }

    private void OnMusicChanged(float value)
    {
        SfxManager.Instance?.SetMusicVolume(value);
    }

    private void OnSfxChanged(float value)
    {
        SfxManager.Instance?.SetSfxVolume(value);
    }

    private static float GetMusicVolume()
    {
        return SfxManager.Instance != null ? SfxManager.Instance.MusicVolume : 0.45f;
    }

    private static float GetSfxVolume()
    {
        return SfxManager.Instance != null ? SfxManager.Instance.SfxVolume : 1f;
    }
}
