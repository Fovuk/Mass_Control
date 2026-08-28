using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;

    private TextMeshProUGUI label;

    public string LocalizationKey => localizationKey;

    void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        LocalizationManager.EnsureInstance();
        LocalizationManager.Instance.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= Refresh;
        }
    }

    public void SetKey(string key)
    {
        localizationKey = key;
        Refresh();
    }

    public void Refresh()
    {
        if (label == null)
        {
            label = GetComponent<TextMeshProUGUI>();
        }

        if (label == null || string.IsNullOrEmpty(localizationKey))
        {
            return;
        }

        if (LocalizationManager.Instance != null &&
            LocalizationManager.Instance.CurrentLanguage == GameLanguage.TR)
        {
            LocalizationManager.ApplyUiFont(label);
        }

        label.text = LocalizationManager.Get(localizationKey);
    }
}
