using UnityEngine;

/// <summary>
/// Caps frame rate and keeps mobile builds off editor Ultra quality defaults.
/// </summary>
public static class PerformanceBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        // Editor/standalone can keep VSync; mobile benefits from an explicit cap.
        Application.targetFrameRate = 60;

#if UNITY_ANDROID || UNITY_IOS
        // Per-platform default is Medium (2); force it in case a build overrides.
        if (QualitySettings.GetQualityLevel() > 2)
        {
            QualitySettings.SetQualityLevel(2, true);
        }

        QualitySettings.vSyncCount = 0;
#endif
    }
}
