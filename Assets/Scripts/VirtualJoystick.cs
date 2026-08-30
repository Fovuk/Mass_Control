using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class VirtualJoystick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [InputControl(layout = "Vector2")]
    [SerializeField] private string controlPath = "<Gamepad>/leftStick";

    [SerializeField] private RectTransform handle;
    [SerializeField] private float movementRange = 50f;

    private RectTransform backgroundRect;
    private Vector2 handleStartPos;
    private Vector2 pointerDownPos;

    protected override string controlPathInternal
    {
        get => controlPath;
        set => controlPath = value;
    }

    void Awake()
    {
        backgroundRect = transform as RectTransform;
    }

    void Start()
    {
        if (handle != null)
        {
            handleStartPos = handle.anchoredPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null || backgroundRect == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundRect,
            eventData.position,
            eventData.pressEventCamera,
            out pointerDownPos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null || backgroundRect == null || handle == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        Vector2 delta = Vector2.ClampMagnitude(localPoint - pointerDownPos, movementRange);
        handle.anchoredPosition = handleStartPos + delta;
        SendValueToControl(new Vector2(delta.x / movementRange, delta.y / movementRange));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (handle != null)
        {
            handle.anchoredPosition = handleStartPos;
        }

        SendValueToControl(Vector2.zero);
    }
}
