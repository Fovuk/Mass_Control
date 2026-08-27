using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlayerJuiceSetup
{
    [MenuItem("Tools/Player/Ensure Juice Components")]
    public static void EnsureComponents()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            EditorApplication.isUpdating)
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
