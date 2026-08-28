using UnityEngine;
using UnityEngine.UI;

public class QuitGameUI : MonoBehaviour
{
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    void Awake()
    {
        if (cancelButton == null)
        {
            cancelButton = transform.Find("ConfirmPanel/CancelButton")?.GetComponent<Button>();
        }

        if (confirmButton == null)
        {
            confirmButton = transform.Find("ConfirmPanel/ConfirmButton")?.GetComponent<Button>();
        }
    }

    void OnEnable()
    {
        cancelButton?.onClick.AddListener(HidePopup);
        confirmButton?.onClick.AddListener(ConfirmQuit);
    }

    void OnDisable()
    {
        cancelButton?.onClick.RemoveListener(HidePopup);
        confirmButton?.onClick.RemoveListener(ConfirmQuit);
    }

    public void ShowPopup()
    {
        gameObject.SetActive(true);
        SfxManager.Instance?.PlayButtonClick();
    }

    public void HidePopup()
    {
        gameObject.SetActive(false);
        SfxManager.Instance?.PlayButtonClick();
    }

    public void ConfirmQuit()
    {
        SfxManager.Instance?.PlayButtonClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
