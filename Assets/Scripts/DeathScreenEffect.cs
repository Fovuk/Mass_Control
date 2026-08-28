using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenEffect : MonoBehaviour
{
    [SerializeField] private Color fadeStartColor = new Color(0.75f, 0.08f, 0.12f, 0f);
    [SerializeField] private Color fadeHoldColor = new Color(0.58f, 0.06f, 0.1f, 0.72f);
    [SerializeField] private float fadeDuration = 0.35f;

    private Image overlay;
    private Coroutine fadeRoutine;
    private bool deathOverlayActive;

    public static void EnsureOn(GameManager manager)
    {
        if (manager == null || manager.GetComponent<DeathScreenEffect>() != null)
        {
            return;
        }

        manager.gameObject.AddComponent<DeathScreenEffect>();
    }

    void Awake()
    {
        EnsureOverlay();
        deathOverlayActive = false;
        ResetOverlay();
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    public void Play()
    {
        EnsureOverlay();
        deathOverlayActive = true;
        overlay.gameObject.SetActive(true);
        overlay.color = fadeStartColor;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeInAndHold());
    }

    public void ResetOverlay()
    {
        deathOverlayActive = false;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (overlay != null)
        {
            overlay.color = fadeStartColor;
            overlay.gameObject.SetActive(false);
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Dead)
        {
            Play();
        }
    }

    private IEnumerator FadeInAndHold()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float smooth = t * t * (3f - 2f * t);
            overlay.color = Color.Lerp(fadeStartColor, fadeHoldColor, smooth);
            yield return null;
        }

        overlay.color = fadeHoldColor;
        fadeRoutine = null;

        while (deathOverlayActive)
        {
            overlay.color = fadeHoldColor;
            yield return null;
        }
    }

    private void EnsureOverlay()
    {
        if (overlay != null)
        {
            return;
        }

        var canvasObject = new GameObject("DeathOverlayCanvas");
        canvasObject.transform.SetParent(transform, false);

        var overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 5000;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>().enabled = false;

        var imageObject = new GameObject("Overlay", typeof(RectTransform));
        imageObject.transform.SetParent(canvasObject.transform, false);

        var rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        overlay = imageObject.AddComponent<Image>();
        overlay.raycastTarget = false;
        overlay.color = fadeStartColor;
        overlay.gameObject.SetActive(false);
    }
}
