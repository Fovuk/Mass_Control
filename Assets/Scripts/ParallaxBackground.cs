using UnityEngine;

[DefaultExecutionOrder(300)]
[DisallowMultipleComponent]
public class ParallaxBackground : MonoBehaviour
{
    private Camera worldCamera;
    private ParallaxLayer[] parallaxLayers;
    private bool initialized;

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
            // Cinemachine bu LateUpdate'te kamerayi oturttuktan sonra kaydet.
            RebakeChildrenToCamera(cameraPosition);
            CacheLayers(cameraPosition);
            initialized = true;
        }

        // Root her frame kameranin gordugu XY'de kalsin.
        transform.position = new Vector3(cameraPosition.x, cameraPosition.y, transform.position.z);

        if (parallaxLayers == null)
        {
            return;
        }

        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            parallaxLayers[i]?.ApplyParallax(cameraPosition);
        }
    }

    private void RebakeChildrenToCamera(Vector3 cameraPosition)
    {
        int childCount = transform.childCount;
        var worldPositions = new Vector3[childCount];
        for (int i = 0; i < childCount; i++)
        {
            worldPositions[i] = transform.GetChild(i).position;
        }

        transform.position = new Vector3(cameraPosition.x, cameraPosition.y, transform.position.z);

        for (int i = 0; i < childCount; i++)
        {
            transform.GetChild(i).position = worldPositions[i];
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
