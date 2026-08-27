using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private PointerDrivenSlider musicSlider;
    [SerializeField] private PointerDrivenSlider sfxSlider;

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
    }

    void OnEnable()
    {
        BindSlider(musicSlider, GetMusicVolume(), OnMusicChanged);
        BindSlider(sfxSlider, GetSfxVolume(), OnSfxChanged);
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

        PlayerPrefs.Save();
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
