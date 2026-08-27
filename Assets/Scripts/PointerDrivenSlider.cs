using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDrivenSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
// custom slider independent from Unity UI Slider
{
    [SerializeField] private float handlePadding = 32f;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private RectTransform handleRect;
    [SerializeField, Range(0f, 1f)] private float value = 0.8f;

    private RectTransform rectTransform;
    public event Action<float> OnValueChanged;
    public float Value => value;

    void Awake()
    {
        rectTransform = transform as RectTransform;
        RefreshVisuals();
    }

    public void SetValueWithoutNotify(float newValue)
    {
        value = Mathf.Clamp01(newValue);
        RefreshVisuals();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Apply(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Apply(eventData);
    }

    private void Apply(PointerEventData eventData)
    {
        if (rectTransform == null)
        {
            return;
        }

        Camera eventCamera = eventData.pressEventCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventCamera, out Vector2 localPoint))
        {
            return;
        }

        Rect rect = rectTransform.rect;
        float minX = rect.xMin + handlePadding;
        float maxX = rect.xMax - handlePadding;
        float newValue = Mathf.Clamp01(Mathf.InverseLerp(minX, maxX, localPoint.x));

        if (Mathf.Approximately(newValue, value))
        {
            return;
        }

        value = newValue;
        RefreshVisuals();
        OnValueChanged?.Invoke(value);
    }

    private void RefreshVisuals()
    {
        if (fillRect != null)
        {
            Vector2 fillAnchorMax = fillRect.anchorMax;
            fillAnchorMax.x = value;
            fillRect.anchorMax = fillAnchorMax;
        }

        if (handleRect != null)
        {
            Vector2 handleAnchorMin = handleRect.anchorMin;
            Vector2 handleAnchorMax = handleRect.anchorMax;
            handleAnchorMin.x = value;
            handleAnchorMax.x = value;
            handleRect.anchorMin = handleAnchorMin;
            handleRect.anchorMax = handleAnchorMax;
        }
    }
}
