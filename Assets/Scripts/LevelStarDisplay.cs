#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class LevelStarDisplay : MonoBehaviour
{
    [SerializeField] private int maxStars = 3;
    [SerializeField] private float starSize = 26f;
    [SerializeField] private float starSpacing = 6f;
    [SerializeField] private float verticalOffset = -18f;
    [SerializeField] private Color filledStarColor = Color.white;
    [SerializeField] private Color emptyStarColor = new Color(0.62f, 0.64f, 0.68f, 1f);

    private Image[] starImages;
    private Sprite emptyStarSprite;
    private Sprite filledStarSprite;
    private int displayedStars;

    void OnEnable()
    {
#if UNITY_EDITOR
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }
#endif

        LoadSprites();
        BuildDisplay();
        ApplyLayout();
        CacheStarImages(force: true);
        ApplyStars();
    }

    public void SetStars(int collected)
    {
        displayedStars = Mathf.Clamp(collected, 0, maxStars);
        ApplyStars();
    }

    private void ApplyStars()
    {
        CacheStarImages();

        if (starImages == null || starImages.Length == 0)
        {
            return;
        }

        LoadSprites();

        if (emptyStarSprite == null || filledStarSprite == null)
        {
            return;
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
            {
                continue;
            }

            bool isFilled = i < displayedStars;
            starImages[i].sprite = isFilled ? filledStarSprite : emptyStarSprite;
            starImages[i].color = isFilled ? filledStarColor : emptyStarColor;
        }
    }

    private void LoadSprites()
    {
        if (emptyStarSprite != null && filledStarSprite != null)
        {
            return;
        }

        emptyStarSprite = StarUiSprites.GetEmptyStar();
        filledStarSprite = StarUiSprites.GetFilledStar();
    }

    private void BuildDisplay()
    {
        if (transform.Find("LevelStars") != null)
        {
            return;
        }

        if (emptyStarSprite == null || filledStarSprite == null)
        {
            return;
        }

        GameObject container = new GameObject("LevelStars", typeof(RectTransform));
        container.transform.SetParent(transform, false);
        container.hideFlags = HideFlags.DontSave;

        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = starSpacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        starImages = new Image[maxStars];

        for (int i = 0; i < maxStars; i++)
        {
            GameObject starObject = new GameObject($"Star_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            starObject.transform.SetParent(container.transform, false);
            starObject.hideFlags = HideFlags.DontSave;

            RectTransform starRect = starObject.GetComponent<RectTransform>();
            starRect.sizeDelta = new Vector2(starSize, starSize);

            Image image = starObject.GetComponent<Image>();
            image.sprite = emptyStarSprite;
            image.color = emptyStarColor;
            image.preserveAspect = true;
            image.raycastTarget = false;

            starImages[i] = image;
        }
    }

    private void ApplyLayout()
    {
        ReserveSpaceForLabel();

        Transform container = transform.Find("LevelStars");
        if (container is not RectTransform containerRect)
        {
            return;
        }

        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0f, verticalOffset);

        float totalWidth = maxStars * starSize + (maxStars - 1) * starSpacing;
        containerRect.sizeDelta = new Vector2(totalWidth, starSize);
    }

    private void CacheStarImages(bool force = false)
    {
        if (!force && starImages != null && starImages.Length > 0)
        {
            return;
        }

        Transform container = transform.Find("LevelStars");
        if (container == null)
        {
            starImages = null;
            return;
        }

        starImages = container.GetComponentsInChildren<Image>(true);
    }

    private void ReserveSpaceForLabel()
    {
        TextMeshProUGUI label = GetComponentInChildren<TextMeshProUGUI>();
        if (label == null)
        {
            return;
        }

        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = new Vector2(0f, -6f);
        label.alignment = TextAlignmentOptions.Center;
    }
}
