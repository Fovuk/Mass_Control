using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class MainMenuButtonIcons : MonoBehaviour
{
    [SerializeField] private Image settingsIcon;
    [SerializeField] private Image quitIcon;

    void Awake()
    {
        if (settingsIcon == null)
        {
            settingsIcon = transform.Find("SettingsButton/Icon")?.GetComponent<Image>();
        }

        if (quitIcon == null)
        {
            quitIcon = transform.Find("QuitButton/Icon")?.GetComponent<Image>();
        }

        ApplyIcon(settingsIcon, "UI/wheel");
        ApplyIcon(quitIcon, "UI/exit-button");
    }

    private static void ApplyIcon(Image target, string resourcePath)
    {
        if (target == null)
        {
            return;
        }

        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            sprite = Resources.Load<Sprite>(resourcePath + "_0");
        }
        if (sprite == null)
        {
            Debug.LogWarning($"[MainMenuButtonIcons] Sprite bulunamadi: {resourcePath}");
            return;
        }

        target.sprite = sprite;
        target.preserveAspect = true;
        target.color = Color.white;
    }
}
