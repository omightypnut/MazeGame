using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f; // degrees per second

    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        // Keep Z at exactly 0
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            0f
        );
    }

    private void FixedUpdate()
    {
        // Move (use rb.velocity if linearVelocity gives an error in your Unity version)
        rb.linearVelocity = input * moveSpeed;

        // Rotate toward movement direction
        if (input.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;

            // If your triangle points UP by default, use -90
            angle -= 90f;

            // Smooth rotate
            float newAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);
        }
    }

}
