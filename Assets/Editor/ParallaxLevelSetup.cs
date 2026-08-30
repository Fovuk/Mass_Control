using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ParallaxLevelSetup
{
    [MenuItem("Tools/Setup Parallax/Log Active Layer World Poses")]
    public static void LogActiveLayerWorldPoses()
    {
        ParallaxBackground background = Object.FindFirstObjectByType<ParallaxBackground>();
        if (background == null)
        {
            Debug.LogError("[ParallaxLevelSetup] ParallaxBackground bulunamadi.");
            return;
        }

        foreach (Transform child in background.transform)
        {
            ParallaxLayer layer = child.GetComponent<ParallaxLayer>();
            Debug.Log(
                $"[Parallax] {child.name} world={child.position} scale={child.localScale} " +
                $"layer={(layer != null ? "yes" : "no")}",
                child);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
