using UnityEngine;

public class VisionCone2D : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float viewRadius = 5f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 90f;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Direction")]
    [SerializeField] private Vector2 localForward = Vector2.right;
    [SerializeField] private float visionAngleOffset = 0f;

    [Header("Adaptive Difficulty")]
    [SerializeField] private float easyViewRadius = 2.5f;
    [SerializeField] private float hardViewRadius = 5f;
    [SerializeField] private float easyViewAngle = 60f;
    [SerializeField] private float hardViewAngle = 110f;

    public bool IsTarget { get; private set; }
    public Transform visibleTarget { get; private set; }

    private void Update()
    {
        if (DifficultyTracker.Instance != null)
        {
            float d = DifficultyTracker.Instance.difficultyScore;
            viewRadius = Mathf.Lerp(easyViewRadius, hardViewRadius, d);
            viewAngle = Mathf.Lerp(easyViewAngle, hardViewAngle, d);
        }

        FindVisibleTarget();
    }

    private void FindVisibleTarget()
    {
        IsTarget = false;
        visibleTarget = null;

        Collider2D[] targetsInRadius = Physics2D.OverlapCircleAll(
            transform.position,
            viewRadius,
            playerMask
        );

        Vector2 worldForward = GetWorldForward();

        foreach (Collider2D targetCollider in targetsInRadius)
        {
            Transform target = targetCollider.transform;
            Vector2 dirToTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;

            float angleToTarget = Vector2.Angle(worldForward, dirToTarget);
            if (angleToTarget > viewAngle * 0.5f)
                continue;

            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                dirToTarget,
                distanceToTarget,
                obstacleMask
            );

            if (hit.collider == null)
            {
                IsTarget = true;
                visibleTarget = target;

                Debug.DrawRay(transform.position, dirToTarget * distanceToTarget, Color.green);
                return;
            }
            else
            {
                Debug.DrawRay(transform.position, dirToTarget * distanceToTarget, Color.red);
            }
        }
    }


    private Vector2 GetWorldForward()
    {
        Transform referenceTransform = transform.parent != null ? transform.parent : transform;
        Vector2 baseForward = referenceTransform.right;
        return Rotate(baseForward, visionAngleOffset).normalized;
    }

    private Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector2 worldForward = Application.isPlaying ? GetWorldForward() : GetEditorWorldForward();

        Vector2 leftBoundary = Rotate(worldForward, -viewAngle * 0.5f);
        Vector2 rightBoundary = Rotate(worldForward, viewAngle * 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightBoundary * viewRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + worldForward * viewRadius);

        if (visibleTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, visibleTarget.position);
        }
    }

    private Vector2 GetEditorWorldForward()
    {
        Transform referenceTransform = transform.parent != null ? transform.parent : transform;
        return ((Vector2)referenceTransform.TransformDirection(localForward)).normalized;
    }

    public bool CanSeeTarget(Transform target)
    {
        if (target == null)
            return false;

        Vector2 worldForward = GetWorldForward();
        Vector2 dirToTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;

        float angleToTarget = Vector2.Angle(worldForward, dirToTarget);
        if (angleToTarget > viewAngle * 0.5f)
            return false;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            dirToTarget,
            distanceToTarget,
            obstacleMask
        );

        return hit.collider == null;
    }

    public float GetViewRadius()
    {
        return viewRadius;
    }

    public float GetViewAngle()
    {
        return viewAngle;
    }
}