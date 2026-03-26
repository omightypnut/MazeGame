using UnityEngine;

public class FogOfWarController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;

    [Header("Fog Settings")]
    [SerializeField] private float visionRadius = 0.12f;
    [SerializeField] private float softness = 0.03f;

    private Material runtimeMaterial;

    private void Awake()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        runtimeMaterial = mr.material; // important: gets runtime instance
    }

    private void LateUpdate()
    {
        if (player == null || mainCamera == null || runtimeMaterial == null)
            return;

        transform.position = new Vector3(
            mainCamera.transform.position.x,
            mainCamera.transform.position.y,
            -1f
        );

        float height = 2f * mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;
        transform.localScale = new Vector3(width, height, 1f);

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(player.position);

        runtimeMaterial.SetVector("_PlayerPos", new Vector4(viewportPos.x, viewportPos.y, 0f, 0f));
        runtimeMaterial.SetFloat("_VisionRadius", visionRadius);
        runtimeMaterial.SetFloat("_Softness", softness);
    }
}