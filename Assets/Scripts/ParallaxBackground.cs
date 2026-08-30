using UnityEngine;

[DefaultExecutionOrder(300)]
[DisallowMultipleComponent]
public class ParallaxBackground : MonoBehaviour
{
    private Camera worldCamera;
    private ParallaxLayer[] parallaxLayers;
    private bool initialized;
    private Vector3 anchorOffset;

    private void LateUpdate()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
            if (worldCamera == null)
            {
                return;
            }
        }

        Vector3 cameraPosition = worldCamera.transform.position;

        if (!initialized)
        {
            // Editörde root'u kameraya göre nereye koyduysan o fark korunur.
            anchorOffset = transform.position - cameraPosition;

            // Cinemachine bu LateUpdate'te kamerayi oturttuktan sonra kaydet.
            CacheLayers(cameraPosition);
            initialized = true;
        }

        // Root kamerayla birlikte hareket eder; editör offset'i silinmez.
        transform.position = cameraPosition + anchorOffset;

        if (parallaxLayers == null)
        {
            return;
        }

        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            parallaxLayers[i]?.ApplyParallax(cameraPosition);
        }
    }

    private void CacheLayers(Vector3 cameraPosition)
    {
        // Only direct layer children — tile clones must not get ParallaxLayer.
        parallaxLayers = GetComponentsInChildren<ParallaxLayer>(true);
        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            parallaxLayers[i]?.Initialize(cameraPosition);
        }
    }
}
