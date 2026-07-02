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

    private Image[] starImages;
    private Sprite emptyStarSprite;
    private Sprite filledStarSprite;

    void OnEnable()
    {
        LoadSprites();
        BuildDisplay();
        ApplyLayout();
        CacheStarImages();
    }

    public void SetStars(int collected)
    {
        CacheStarImages();

        if (starImages == null)
        {
            return;
        }

        collected = Mathf.Clamp(collected, 0, maxStars);

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
            {
                continue;
            }

            starImages[i].sprite = i < collected ? filledStarSprite : emptyStarSprite;
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

    private void CacheStarImages()
    {
        if (starImages != null && starImages.Length > 0)
        {
            return;
        }

        Transform container = transform.Find("LevelStars");
        if (container == null)
        {
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
