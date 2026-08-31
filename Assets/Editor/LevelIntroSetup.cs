using UnityEditor;
using UnityEngine;
using TMPro;

public static class LevelIntroSetup
{
    const string SettingsPath = "Assets/Resources/LevelIntroSettings.asset";
    const string HogfishFontPath = "Assets/kenney_ui-pack/Font/Hogfish DEMO SDF.asset";
    const string SubtitleFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [MenuItem("Tools/Setup Level Intro Settings")]
    public static void SetupSettingsAsset()
    {
        var settings = AssetDatabase.LoadAssetAtPath<LevelIntroSettings>(SettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<LevelIntroSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }

        settings.titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HogfishFontPath);
        settings.subtitleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SubtitleFontPath);

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Debug.Log("[LevelIntroSetup] LevelIntroSettings asset hazir.");
    }
}
