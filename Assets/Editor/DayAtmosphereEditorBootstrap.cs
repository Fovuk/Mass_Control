using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DayAtmosphereEditorBootstrap
{
    static DayAtmosphereEditorBootstrap()
    {
        EditorApplication.delayCall += ApplyActiveScene;
        EditorSceneManager.sceneOpened -= HandleSceneOpened;
        EditorSceneManager.sceneOpened += HandleSceneOpened;
    }

    private static void HandleSceneOpened(Scene scene, OpenSceneMode mode)
    {
        Apply(scene);
    }

    private static void ApplyActiveScene()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Apply(SceneManager.GetActiveScene());
        }
    }

    private static void Apply(Scene scene)
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (DayAtmosphereController.ApplyToScene(scene, false))
        {
            EditorSceneManager.MarkSceneDirty(scene);
            SceneView.RepaintAll();
        }
    }
}
