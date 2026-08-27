using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Migrates loaded Player objects from runtime-only juice components to
// persistent scene components, so their settings are editable and saveable.
[InitializeOnLoad]
public static class PlayerJuiceSetup
{
    static PlayerJuiceSetup()
    {
        EditorApplication.delayCall += EnsureComponents;
        EditorApplication.playModeStateChanged += HandlePlayModeChanged;
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        EditorApplication.delayCall += EnsureComponents;
    }

    private static void HandlePlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.delayCall += EnsureComponents;
        }
    }

    [MenuItem("Tools/Player/Ensure Juice Components")]
    public static void EnsureComponents()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        PlayerController[] players =
            Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include);

        foreach (PlayerController player in players)
        {
            if (!player.gameObject.scene.IsValid())
            {
                continue;
            }

            bool changed = false;

            if (player.GetComponent<PlayerSquashStretch>() == null)
            {
                Undo.AddComponent<PlayerSquashStretch>(player.gameObject);
                changed = true;
            }

            if (player.GetComponent<PlayerLandingJuice>() == null)
            {
                Undo.AddComponent<PlayerLandingJuice>(player.gameObject);
                changed = true;
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
                EditorUtility.SetDirty(player.gameObject);
            }
        }
    }
}
