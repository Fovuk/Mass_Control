using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MainMenuHeaderEffect : MonoBehaviour
{
    [SerializeField] private float bounceHeight = 14f;
    [SerializeField] private float bounceSpeed = 1.8f;

    private RectTransform rectTransform;
    private Vector2 basePosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        basePosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.unscaledTime * bounceSpeed) * bounceHeight;
        rectTransform.anchoredPosition = basePosition + new Vector2(0f, offsetY);
    }
}
