using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level6CameraLookAhead : MonoBehaviour
{
    private const string Level6SceneName = "Level_06";

    [Header("Level 6 Camera")]
    [Tooltip("Look-ahead only. Orthographic size sahne uzerindeki Cinemachine Lens degerinden okunur; runtime'da degistirilmez.")]
    [SerializeField, Min(0f)] private float extraViewSize = 0f;
    [SerializeField, Min(0f)] private float maximumLookAhead = 4.5f;
    [SerializeField, Min(0f)] private float minimumLookAheadSpeed = 1.5f;
    [SerializeField, Min(0.01f)] private float fullLookAheadSpeed = 8f;
    [SerializeField, Min(0.01f)] private float moveSmoothTime = 0.28f;
    [SerializeField, Min(0.01f)] private float returnSmoothTime = 0.45f;

    private CinemachineCamera virtualCamera;
    private CinemachineFollow cameraFollow;
    private Rigidbody2D playerBody;
    private float centeredOffsetX;
    private float currentOffsetX;
    private float offsetVelocity;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneCallback()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != Level6SceneName)
        {
            return;
        }

        CinemachineCamera camera = Object.FindAnyObjectByType<CinemachineCamera>();
        if (camera != null && camera.GetComponent<Level6CameraLookAhead>() == null)
        {
            camera.gameObject.AddComponent<Level6CameraLookAhead>();
        }
    }

    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != Level6SceneName)
        {
            enabled = false;
            return;
        }

        virtualCamera = GetComponent<CinemachineCamera>();
        cameraFollow = GetComponent<CinemachineFollow>();

        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            playerBody = player.GetComponent<Rigidbody2D>();
        }

        if (virtualCamera == null || cameraFollow == null || playerBody == null)
        {
            Debug.LogWarning(
                "[Level6CameraLookAhead] Camera follow or player was not found.",
                this);
            enabled = false;
            return;
        }

        centeredOffsetX = cameraFollow.FollowOffset.x;
        currentOffsetX = centeredOffsetX;

        // Orthographic size yalnizca sahne (ve dolayisiyla editör) uzerinden ayarlanir.
        // Runtime'da buyutmek parallax arkaplanin editore gore kucuk gorunmesine yol aciyordu.
        if (extraViewSize > 0f)
        {
            LensSettings lens = virtualCamera.Lens;
            lens.OrthographicSize += extraViewSize;
            virtualCamera.Lens = lens;
        }
    }

    private void LateUpdate()
    {
        float horizontalSpeed = playerBody.linearVelocity.x;
        float absoluteSpeed = Mathf.Abs(horizontalSpeed);
        float targetOffsetX = centeredOffsetX;

        if (absoluteSpeed > minimumLookAheadSpeed)
        {
            float speedRange = Mathf.Max(
                0.01f,
                fullLookAheadSpeed - minimumLookAheadSpeed);
            float lookAmount = Mathf.Clamp01(
                (absoluteSpeed - minimumLookAheadSpeed) / speedRange);
            targetOffsetX += Mathf.Sign(horizontalSpeed) *
                             maximumLookAhead *
                             lookAmount;
        }

        float smoothTime = Mathf.Approximately(targetOffsetX, centeredOffsetX)
            ? returnSmoothTime
            : moveSmoothTime;

        currentOffsetX = Mathf.SmoothDamp(
            currentOffsetX,
            targetOffsetX,
            ref offsetVelocity,
            smoothTime);

        Vector3 followOffset = cameraFollow.FollowOffset;
        followOffset.x = currentOffsetX;
        cameraFollow.FollowOffset = followOffset;
    }

}
