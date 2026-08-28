using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelWakeUpEffect : MonoBehaviour
{
    [SerializeField] private Color eyelidColor = new Color(0.015f, 0.015f, 0.04f, 1f);
    [SerializeField] private float openDuration = 0.9f;
    [SerializeField] private float startDelay = 0.04f;
    [SerializeField] private int overlaySortOrder = 4900;

    private Canvas overlayCanvas;
    private RectTransform topEyelid;
    private RectTransform bottomEyelid;
    private Coroutine openRoutine;

    public static void EnsureOn(GameManager manager)
    {
        if (manager == null || manager.GetComponent<LevelWakeUpEffect>() != null)
        {
            return;
        }

        manager.gameObject.AddComponent<LevelWakeUpEffect>();
    }

    void Awake()
    {
        EnsureOverlay();
        SetClosedImmediate();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex <= 0)
        {
            HideOverlay();
            return;
        }

        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
        }

        openRoutine = StartCoroutine(PlayOpen());
    }

    private IEnumerator PlayOpen()
    {
        overlayCanvas.gameObject.SetActive(true);
        SetClosedImmediate();

        if (startDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(startDelay);
        }

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            ApplyOpenAmount(eased * 0.5f);
            yield return null;
        }

        HideOverlay();
        openRoutine = null;
    }

    private void SetClosedImmediate()
    {
        ApplyOpenAmount(0f);
    }

    private void ApplyOpenAmount(float openHalf)
    {
        if (topEyelid == null || bottomEyelid == null)
        {
            return;
        }

        topEyelid.anchorMin = new Vector2(0f, 0.5f + openHalf);
        topEyelid.anchorMax = Vector2.one;
        bottomEyelid.anchorMin = Vector2.zero;
        bottomEyelid.anchorMax = new Vector2(1f, 0.5f - openHalf);
    }

    private void HideOverlay()
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(false);
        }
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        var canvasObject = new GameObject("WakeUpOverlayCanvas");
        canvasObject.transform.SetParent(transform, false);

        overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = overlaySortOrder;
        overlayCanvas.pixelPerfect = false;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>().enabled = false;

        topEyelid = CreateEyelid(canvasObject.transform, "TopEyelid");
        bottomEyelid = CreateEyelid(canvasObject.transform, "BottomEyelid");
    }

    private RectTransform CreateEyelid(Transform parent, string objectName)
    {
        var eyelidObject = new GameObject(objectName, typeof(RectTransform));
        eyelidObject.transform.SetParent(parent, false);

        var rect = eyelidObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = eyelidObject.AddComponent<Image>();
        image.color = eyelidColor;
        image.raycastTarget = false;

        return rect;
    }
}
