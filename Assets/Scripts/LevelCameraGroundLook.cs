using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CinemachineFollow))]
public class LevelCameraGroundLook : MonoBehaviour
{
    [Header("Grounded Vertical Look (Level 1-5)")]
    [Tooltip("Oyuncu zemindeyken kameranin yukariya kayacagi mesafe.")]
    [SerializeField, Min(0f)] private float groundedLookUpOffset = 2.25f;

    [Tooltip("Zeminde yukariya bakis gecis suresi. Dusuk deger daha hizlidir.")]
    [SerializeField, Min(0.01f)] private float groundedTransitionTime = 1.60f;

    [Tooltip("Oyuncu havadayken merkeze donus suresi.")]
    [SerializeField, Min(0.01f)] private float airborneReturnTime = 1.20f;

    [Tooltip("Kameranin dikey olarak bir saniyede gidebilecegi maksimum mesafe.")]
    [SerializeField, Min(0.1f)] private float maximumVerticalSpeed = 1.25f;

    private CinemachineFollow cameraFollow;
    private PlayerController player;
    private float centeredOffsetY;
    private float currentOffsetY;
    private float verticalVelocity;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneCallback()
    {
        SceneManager.sceneLoaded -= AddToSupportedLevel;
        SceneManager.sceneLoaded += AddToSupportedLevel;
    }

    private static void AddToSupportedLevel(Scene scene, LoadSceneMode mode)
    {
        if (!IsSupportedLevel(scene.name))
        {
            return;
        }

        CinemachineCamera camera = Object.FindAnyObjectByType<CinemachineCamera>();
        if (camera != null && camera.GetComponent<LevelCameraGroundLook>() == null)
        {
            camera.gameObject.AddComponent<LevelCameraGroundLook>();
        }
    }

    private void Awake()
    {
        if (!IsSupportedLevel(SceneManager.GetActiveScene().name))
        {
            enabled = false;
            return;
        }

        cameraFollow = GetComponent<CinemachineFollow>();
        player = Object.FindAnyObjectByType<PlayerController>();

        if (cameraFollow == null || player == null)
        {
            Debug.LogWarning(
                "[LevelCameraGroundLook] Cinemachine Follow veya Player bulunamadi.",
                this);
            enabled = false;
            return;
        }

        centeredOffsetY = cameraFollow.FollowOffset.y;
        currentOffsetY = centeredOffsetY;
    }

    private void LateUpdate()
    {
        float targetOffsetY = centeredOffsetY;
        float smoothTime = airborneReturnTime;

        if (player.IsGrounded)
        {
            targetOffsetY += groundedLookUpOffset;
            smoothTime = groundedTransitionTime;
        }

        currentOffsetY = Mathf.SmoothDamp(
            currentOffsetY,
            targetOffsetY,
            ref verticalVelocity,
            smoothTime,
            maximumVerticalSpeed);

        Vector3 followOffset = cameraFollow.FollowOffset;
        followOffset.y = currentOffsetY;
        cameraFollow.FollowOffset = followOffset;
    }

    private void OnValidate()
    {
        groundedLookUpOffset = Mathf.Max(0f, groundedLookUpOffset);
        groundedTransitionTime = Mathf.Max(0.01f, groundedTransitionTime);
        airborneReturnTime = Mathf.Max(0.01f, airborneReturnTime);
        maximumVerticalSpeed = Mathf.Max(0.1f, maximumVerticalSpeed);
    }

    private static bool IsSupportedLevel(string sceneName)
    {
        if (!sceneName.StartsWith("Level_") ||
            !int.TryParse(sceneName.Substring("Level_".Length), out int level))
        {
            return false;
        }

        return level >= 1 && level <= 5;
    }
}
