using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
public class SpikeTilemapHazard : MonoBehaviour
{
    private void Reset()
    {
        ConfigureHazard();
    }

    private void Awake()
    {
        ConfigureHazard();
    }

    private void ConfigureHazard()
    {
        gameObject.tag = "Trap";

        TilemapCollider2D tilemapCollider = GetComponent<TilemapCollider2D>();
        tilemapCollider.isTrigger = true;
    }
}
