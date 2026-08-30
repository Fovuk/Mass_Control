using UnityEngine;

[DisallowMultipleComponent]
public class ParallaxLayer : MonoBehaviour
{
    [Tooltip("Yatay parallax. 1 = ekranda neredeyse sabit (uzak katman). Dusuk = ekranda hizli kayar (yakin katman).")]
    [Range(0f, 1f)]
    [SerializeField] private float parallaxFactor = 1f;

    [SerializeField] private int horizontalTileRadius = 2;

    private Vector3 initialLocalPosition;
    private Vector3 initialCameraPosition;
    private float tileWidthRootSpace;
    private bool initialized;

    public void Initialize(Vector3 cameraPosition)
    {
        BuildHorizontalTiles();

        // Period in ParallaxBackground local space (root scale is 1).
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        float spriteWidth = renderer != null && renderer.sprite != null
            ? renderer.sprite.bounds.size.x
            : 1f;
        tileWidthRootSpace = Mathf.Max(spriteWidth * Mathf.Abs(transform.localScale.x), 0.01f);

        initialLocalPosition = transform.localPosition;
        initialCameraPosition = cameraPosition;
        initialized = true;
    }

    public void ApplyParallax(Vector3 cameraPosition)
    {
        if (!initialized)
        {
            Initialize(cameraPosition);
        }

        Vector3 cameraDelta = cameraPosition - initialCameraPosition;

        // X: parallax lag + wrap (infinite horizontal)
        float offsetX = Wrap(cameraDelta.x * (parallaxFactor - 1f), tileWidthRootSpace);

        // Y: always lock to camera so the layer never reveals empty sky above/below.
        float offsetY = 0f;

        transform.localPosition = new Vector3(
            initialLocalPosition.x + offsetX,
            initialLocalPosition.y + offsetY,
            initialLocalPosition.z);
    }

    private void BuildHorizontalTiles()
    {
        SpriteRenderer source = GetComponent<SpriteRenderer>();
        if (source == null || source.sprite == null || horizontalTileRadius <= 0)
        {
            return;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.EndsWith("_Tile"))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // Child local units are pre-scale; world spacing becomes spriteWidth * scale.
        float spriteWidth = source.sprite.bounds.size.x;
        for (int i = 1; i <= horizontalTileRadius; i++)
        {
            CreateTile(source, -spriteWidth * i);
            CreateTile(source, spriteWidth * i);
        }
    }

    private void CreateTile(SpriteRenderer source, float localX)
    {
        var tile = new GameObject(source.sprite.name + "_Tile");
        tile.transform.SetParent(transform, false);
        tile.transform.localPosition = new Vector3(localX, 0f, 0f);
        tile.transform.localRotation = Quaternion.identity;
        tile.transform.localScale = Vector3.one;

        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = source.sprite;
        renderer.color = source.color;
        renderer.sortingLayerID = source.sortingLayerID;
        renderer.sortingOrder = source.sortingOrder;
        renderer.sharedMaterial = source.sharedMaterial;
        renderer.flipX = source.flipX;
        renderer.flipY = source.flipY;
    }

    private static float Wrap(float value, float period)
    {
        if (period <= 0.01f)
        {
            return value;
        }

        return Mathf.Repeat(value + period * 0.5f, period) - period * 0.5f;
    }
}
