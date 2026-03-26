using UnityEngine;
using UnityEngine.UI;

public class FogOfWarUIController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RawImage rawImage;

    [Header("Fog Settings")]
    [SerializeField] private float visionRadius = 0.15f;
    [SerializeField] private float softness = 0.03f;

    private Material runtimeMaterial;

    private void Awake()
    {
        runtimeMaterial = Instantiate(rawImage.material);
        rawImage.material = runtimeMaterial;
    }

    private void LateUpdate()
    {
        if (player == null || mainCamera == null || rawImage == null || runtimeMaterial == null)
            return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(player.position);

        runtimeMaterial.SetVector("_PlayerPos", new Vector4(viewportPos.x, viewportPos.y, 0f, 0f));
        runtimeMaterial.SetFloat("_VisionRadius", visionRadius);
        runtimeMaterial.SetFloat("_Softness", softness);
    }
}