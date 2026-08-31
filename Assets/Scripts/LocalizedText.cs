using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    private static readonly HashSet<string> PreserveSceneFontKeys = new()
    {
        "game_title",
    };

    [SerializeField] private string localizationKey;
    [SerializeField] private bool preserveSceneFont;

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
            LocalizationManager.Instance.CurrentLanguage == GameLanguage.TR &&
            !preserveSceneFont &&
            !PreserveSceneFontKeys.Contains(localizationKey))
        {
            LocalizationManager.ApplyUiFont(label);
        }

        label.text = LocalizationManager.Get(localizationKey);
    }
}
