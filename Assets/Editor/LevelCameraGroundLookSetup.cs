using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class LevelCameraGroundLookSetup
{
    static LevelCameraGroundLookSetup()
    {
        EditorApplication.delayCall += AddToActiveScene;
        EditorSceneManager.sceneOpened -= HandleSceneOpened;
        EditorSceneManager.sceneOpened += HandleSceneOpened;
    }

    private static void AddToActiveScene()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            AddToScene(SceneManager.GetActiveScene());
        }
    }

    private static void HandleSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (!Application.isPlaying)
        {
            AddToScene(scene);
        }
    }

    private static void AddToScene(Scene scene)
    {
        if (!IsLevelOneToFive(scene.name))
        {
            return;
        }

        foreach (CinemachineCamera camera in Object.FindObjectsByType<CinemachineCamera>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (camera.gameObject.scene != scene)
            {
                continue;
            }

            LevelCameraGroundLook groundLook =
                camera.GetComponent<LevelCameraGroundLook>();

            if (groundLook == null)
            {
                groundLook = Undo.AddComponent<LevelCameraGroundLook>(camera.gameObject);
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log(
                    $"[LevelCameraGroundLookSetup] Ground look eklendi: {scene.name}");
            }
            else if (UpgradeLegacyTransitionTimes(groundLook))
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            break;
        }
    }

    private static bool UpgradeLegacyTransitionTimes(LevelCameraGroundLook groundLook)
    {
        var serializedLook = new SerializedObject(groundLook);
        SerializedProperty groundedTime =
            serializedLook.FindProperty("groundedTransitionTime");
        SerializedProperty airborneTime =
            serializedLook.FindProperty("airborneReturnTime");
        bool changed = false;

        if (groundedTime != null &&
            (Mathf.Approximately(groundedTime.floatValue, 0.38f) ||
             Mathf.Approximately(groundedTime.floatValue, 0.80f)))
        {
            groundedTime.floatValue = 1.60f;
            changed = true;
        }

        if (airborneTime != null &&
            (Mathf.Approximately(airborneTime.floatValue, 0.32f) ||
             Mathf.Approximately(airborneTime.floatValue, 0.65f)))
        {
            airborneTime.floatValue = 1.20f;
            changed = true;
        }

        if (changed)
        {
            serializedLook.ApplyModifiedProperties();
        }

        return changed;
    }

    private static bool IsLevelOneToFive(string sceneName)
    {
        return sceneName.StartsWith("Level_") &&
               int.TryParse(sceneName.Substring("Level_".Length), out int level) &&
               level >= 1 &&
               level <= 5;
    }
}
