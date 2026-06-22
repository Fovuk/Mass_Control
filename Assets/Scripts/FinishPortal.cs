using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FinishPortal : MonoBehaviour
{
    void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        Debug.Log($"[FinishPortal] Hazir: {name}, IsTrigger={collider.isTrigger}, Tag={tag}");

        if (!collider.isTrigger)
        {
            Debug.LogWarning("[FinishPortal] Box Collider 2D uzerinde Is Trigger acik olmali.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[FinishPortal] Trigger algilandi -> Obje: {other.name}, Tag: {other.tag}");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[FinishPortal] Reddedildi: '{other.tag}' tag'i Player degil.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[FinishPortal] GameManager bulunamadi!");
            return;
        }

        Debug.Log("[FinishPortal] Player girisi onaylandi, CompleteLevel cagriliyor.");
        GameManager.Instance.CompleteLevel();
    }
}
