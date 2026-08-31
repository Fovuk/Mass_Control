using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelIntroSettings", menuName = "UI/Level Intro Settings")]
public class LevelIntroSettings : ScriptableObject
{
    public TMP_FontAsset titleFont;
    public TMP_FontAsset subtitleFont;

    private static LevelIntroSettings cached;

    public static LevelIntroSettings Get()
    {
        if (cached == null)
        {
            cached = Resources.Load<LevelIntroSettings>("LevelIntroSettings");
        }

        return cached;
    }
}
