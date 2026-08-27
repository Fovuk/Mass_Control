using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonClickSfx : MonoBehaviour, IPointerDownHandler, ISubmitHandler
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && button.IsInteractable())
        {
            SfxManager.Instance?.PlayButtonClick();
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (button != null && button.IsInteractable())
        {
            SfxManager.Instance?.PlayButtonClick();
        }
    }
}
