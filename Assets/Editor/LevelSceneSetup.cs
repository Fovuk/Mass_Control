using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelSceneSetup
{
    [MenuItem("Tools/Unlock All Levels (Debug)")]
    public static void UnlockAllLevelsDebug()
    {
        PlayerPrefs.SetInt("HighestUnlockedLevel", 6);
        for (int i = 1; i <= 6; i++)
            PlayerPrefs.SetInt($"LevelStars_{i}", 3);
        PlayerPrefs.Save();
        Debug.Log("[LevelSceneSetup] All levels unlocked with 3 stars each (debug).");
    }

    [MenuItem("Tools/Validate Active Level")]
    public static void ValidateActiveLevel()
    {
        var stars = GameObject.FindGameObjectsWithTag("Star");
        var finish = GameObject.FindGameObjectWithTag("Finish");
        var player = GameObject.FindGameObjectWithTag("Player");
        var tilemaps = Object.FindObjectsByType<UnityEngine.Tilemaps.Tilemap>();

        Debug.Log($"[LevelSceneSetup] Stars: {stars.Length} (expected 3)");
        Debug.Log($"[LevelSceneSetup] Finish portal: {(finish != null ? "OK" : "MISSING")}");
        Debug.Log($"[LevelSceneSetup] Player: {(player != null ? "OK" : "MISSING")}");
        Debug.Log($"[LevelSceneSetup] Tilemaps: {tilemaps.Length}");

        if (stars.Length != 3)
            Debug.LogWarning("[LevelSceneSetup] Level must contain exactly 3 Star-tagged objects.");
    }

    [MenuItem("Tools/Open Level Scene/Level 3")]
    public static void OpenLevel3() => OpenLevel(3);

    [MenuItem("Tools/Open Level Scene/Level 4")]
    public static void OpenLevel4() => OpenLevel(4);

    [MenuItem("Tools/Open Level Scene/Level 5")]
    public static void OpenLevel5() => OpenLevel(5);

    [MenuItem("Tools/Open Level Scene/Level 6")]
    public static void OpenLevel6() => OpenLevel(6);

    static void OpenLevel(int levelNumber)
    {
        var path = $"Assets/Scenes/Level_{levelNumber:D2}.unity";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError($"[LevelSceneSetup] Scene not found: {path}");
            return;
        }

        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }
}
