using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MovingSpikeAuthoringTools
{
    private const string SpikeSpritePath =
        "Assets/Free Platform Game Assets/Platform Game Assets/Enemies/png/256x256/Mace.png";

    [MenuItem("Tools/Moving Spikes/Add One Rig")]
    public static void AddOneRig()
    {
        Vector3 center = GetPlacementCenter();
        CreateRig(center, 8f, 6f);
        MarkSceneDirty();
    }

    [MenuItem("Tools/Moving Spikes/Add Five Rigs")]
    public static void AddFiveRigs()
    {
        Vector3 center = GetPlacementCenter();
        float[] speeds = { 7f, 9f, 11f, 13f, 15f };

        for (int i = 0; i < speeds.Length; i++)
        {
            Vector3 position = center + new Vector3(i * 12f, i % 2 == 0 ? 0f : 4f, 0f);
            CreateRig(position, 8f + i, speeds[i]);
        }

        MarkSceneDirty();
        Debug.Log("[MovingSpikeAuthoringTools] Added five moving-spike rigs.");
    }

    private static void CreateRig(Vector3 center, float pathWidth, float speed)
    {
        Transform spikesParent = FindOrCreateRoot("Spikes");
        Transform waypointsParent = FindOrCreateRoot("MovingSpikeWaypoints");
        int number = GetNextSpikeNumber();

        GameObject pointA = CreateWaypoint(
            $"Spike_{number:D2}_PointA",
            center + Vector3.left * (pathWidth * 0.5f),
            waypointsParent);
        GameObject pointB = CreateWaypoint(
            $"Spike_{number:D2}_PointB",
            center + Vector3.right * (pathWidth * 0.5f),
            waypointsParent);

        var spike = new GameObject($"Moving_Spike ({number})");
        Undo.RegisterCreatedObjectUndo(spike, "Create moving spike");
        spike.tag = "Trap";
        spike.transform.SetParent(spikesParent, true);
        spike.transform.position = center;
        spike.transform.localScale = new Vector3(1.32f, 1.09f, 1f);

        var body = Undo.AddComponent<Rigidbody2D>(spike);
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = Undo.AddComponent<BoxCollider2D>(spike);
        collider.isTrigger = true;
        collider.offset = new Vector2(0f, 0.1f);
        collider.size = new Vector2(2.34f, 2.27f);

        var renderer = Undo.AddComponent<SpriteRenderer>(spike);
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpikeSpritePath);
        renderer.color = new Color(1f, 0.48f, 0.48f, 1f);

        var mover = Undo.AddComponent<MovingPlatform>(spike);
        var serializedMover = new SerializedObject(mover);
        serializedMover.FindProperty("pointA").objectReferenceValue = pointA.transform;
        serializedMover.FindProperty("pointB").objectReferenceValue = pointB.transform;
        serializedMover.FindProperty("speed").floatValue = speed;
        serializedMover.FindProperty("startAtPointA").boolValue = true;
        serializedMover.ApplyModifiedPropertiesWithoutUndo();

        Selection.activeGameObject = spike;
    }

    private static GameObject CreateWaypoint(
        string waypointName,
        Vector3 position,
        Transform parent)
    {
        var waypoint = new GameObject(waypointName);
        Undo.RegisterCreatedObjectUndo(waypoint, "Create spike waypoint");
        waypoint.transform.SetParent(parent, true);
        waypoint.transform.position = position;
        return waypoint;
    }

    private static int GetNextSpikeNumber()
    {
        int highestNumber = 0;
        MovingPlatform[] movers = Object.FindObjectsByType<MovingPlatform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (MovingPlatform mover in movers)
        {
            int openParenthesis = mover.name.LastIndexOf('(');
            int closeParenthesis = mover.name.LastIndexOf(')');
            if (openParenthesis < 0 || closeParenthesis <= openParenthesis)
            {
                continue;
            }

            string numberText = mover.name.Substring(
                openParenthesis + 1,
                closeParenthesis - openParenthesis - 1);
            if (int.TryParse(numberText, out int number))
            {
                highestNumber = Mathf.Max(highestNumber, number);
            }
        }

        return highestNumber + 1;
    }

    private static Transform FindOrCreateRoot(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null && existing.scene == SceneManager.GetActiveScene())
        {
            return existing.transform;
        }

        var root = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(root, $"Create {objectName}");
        return root.transform;
    }

    private static Vector3 GetPlacementCenter()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            Vector3 pivot = SceneView.lastActiveSceneView.pivot;
            return new Vector3(
                Mathf.Round(pivot.x),
                Mathf.Round(pivot.y),
                0f);
        }

        return Vector3.zero;
    }

    private static void MarkSceneDirty()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
