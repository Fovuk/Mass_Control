using TMPro;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starText;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameUI: Sahnedeki GameManager objesi bulunamadi.");
            return;
        }

        GameManager.Instance.OnStarsChanged += HandleStarsChanged;
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        HandleStarsChanged(GameManager.Instance.CollectedStars, GameManager.Instance.MaxStarsPerLevel);
        HandleGameStateChanged(GameManager.Instance.CurrentState);
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnStarsChanged -= HandleStarsChanged;
        GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleStarsChanged(int collected, int max)
    {
        if (starText != null)
        {
            starText.text = "Yildiz: " + collected + " / " + max;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        // Menu, pause, level complete ekranlari burada yonetilebilir.
    }
}
