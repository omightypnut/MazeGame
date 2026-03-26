using UnityEngine;
using MarwanZaky.MathfX;
using MarwanZaky.Methods;

public class FieldOfView : MonoBehaviour
{
    Mesh mesh;

    Vector3 origin = Vector3.zero;

    float startAngle;

    [SerializeField] MeshFilter meshFilter;
    [SerializeField] int rayCount = 2;
    [SerializeField] float fieldOfView = 90f;
    [SerializeField] float distance = 50f;
    [SerializeField] Vector3 offset;
    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask layerMask;

    public bool IsTarget { get; private set; }

    private void Start()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    private void LateUpdate()
    {

        transform.position = transform.parent.position;
        transform.rotation = transform.parent.rotation;

        IsTarget = false;
        origin = new Vector3(
            transform.parent.position.x,
            transform.parent.position.y,
            0f
            );
            //transform.position = Vector3.zero;
            //transform.rotation = Quaternion.identity;

            var angle = startAngle;
        var angleIncrease = fieldOfView / rayCount;
        var vertices = new Vector3[rayCount + 1 + 1];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[rayCount * 3];
        var vertexIndex = 1;
        var trianglesIndex = 0;

        vertices[0] = origin;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector2 direction = transform.parent.TransformDirection(new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ));

            // Use Physics2D instead of PhysicsX
            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(origin.x, origin.y),
                direction,
                distance,
                layerMask | targetMask
            );

            Vector3 vertex;
            if (hit.collider != null)
            {
                vertex = new Vector3(hit.point.x, hit.point.y, origin.z);

                // Check if we hit the player (target layer)
                if (CompareLayer(hit.collider.gameObject.layer, targetMask))
                    IsTarget = true;
            }
            else
            {
                vertex = origin + new Vector3(
                    direction.x, direction.y, 0) * distance;
            }

            // Debug ray
            Debug.DrawRay(
                new Vector3(origin.x, origin.y, 0),
                new Vector3(direction.x, direction.y, 0) * distance,
                IsTarget ? Color.green : Color.red
            );

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[trianglesIndex + 0] = 0;
                triangles[trianglesIndex + 1] = vertexIndex - 1;
                triangles[trianglesIndex + 2] = vertexIndex;
                trianglesIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin + offset;
    }

    public void SetDirection(Vector3 direction)
    {
        startAngle = MathfX.VectorToAngle3D(direction) + fieldOfView / 2f;
    }

    bool CompareLayer(LayerMask layer, LayerMask layerMask)
    {
        return layerMask == (layerMask | (1 << layer));
    }
}