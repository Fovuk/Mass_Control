using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Auto-applies Mass Control product name, landscape-only orientation, and app icon
/// when the editor loads. Also available under Tools/Mass Control if needed.
/// </summary>
[InitializeOnLoad]
public static class MassControlBrandingSetup
{
    const string IconPath = "Assets/Art/Branding/MassControl_AppIcon.jpg";
    const string ProductName = "Mass Control";
    const string PrefKey = "MassControl.BrandingApplied.v2";

    static MassControlBrandingSetup()
    {
        EditorApplication.delayCall += TryAutoApply;
    }

    static void TryAutoApply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += TryAutoApply;
            return;
        }

        if (EditorPrefs.GetBool(PrefKey, false) && HasDefaultIconAssigned())
        {
            return;
        }

        if (ApplyInternal(logErrors: false))
        {
            EditorPrefs.SetBool(PrefKey, true);
            Debug.Log("[Mass Control] Branding applied automatically (name, landscape, app icon).");
        }
    }

    [MenuItem("Tools/Mass Control/Apply Branding Settings")]
    public static void ApplyFromMenu()
    {
        if (ApplyInternal(logErrors: true))
        {
            EditorPrefs.SetBool(PrefKey, true);
            Debug.Log("[Mass Control] Branding applied.");
        }
    }

    static bool ApplyInternal(bool logErrors)
    {
        PlayerSettings.productName = ProductName;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (icon == null)
        {
            if (logErrors)
            {
                Debug.LogError($"[Mass Control] Icon not found at {IconPath}");
            }

            return false;
        }

        // Default icon used by builds when platform-specific slots are empty.
        PlayerSettings.SetIcons(
            NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Unknown),
            new[] { icon },
            IconKind.Any);

        TryAssignPlatformIcons(NamedBuildTarget.Standalone, icon);
        TryAssignPlatformIcons(NamedBuildTarget.Android, icon);
        TryAssignPlatformIcons(NamedBuildTarget.iOS, icon);

        AssetDatabase.SaveAssets();
        return true;
    }

    static bool HasDefaultIconAssigned()
    {
        Texture2D[] icons = PlayerSettings.GetIcons(
            NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Unknown),
            IconKind.Any);
        return icons != null && icons.Length > 0 && icons[0] != null;
    }

    static void TryAssignPlatformIcons(NamedBuildTarget target, Texture2D icon)
    {
        try
        {
            PlatformIconKind[] kinds = PlayerSettings.GetSupportedIconKinds(target);
            foreach (PlatformIconKind kind in kinds)
            {
                PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(target, kind);
                for (int i = 0; i < icons.Length; i++)
                {
                    int layerCount = icons[i].maxLayerCount;
                    if (layerCount <= 1)
                    {
                        icons[i].SetTexture(icon);
                    }
                    else
                    {
                        var layers = new Texture2D[layerCount];
                        for (int layer = 0; layer < layerCount; layer++)
                        {
                            layers[layer] = icon;
                        }

                        icons[i].SetTextures(layers);
                    }
                }

                PlayerSettings.SetPlatformIcons(target, kind, icons);
            }
        }
        catch (System.Exception)
        {
            // Platform module may be missing (e.g. iOS on Windows).
        }
    }
}
