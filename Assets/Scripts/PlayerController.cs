using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundedDistanceThreshold = 0.1f; // Distance threshold to be considered grounded

    [Header("Circular World")]
    [SerializeField] private Transform worldCenter;
    [SerializeField] private float worldRadius = 10f;
    [SerializeField] private float gravityStrength = 20f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;
    private bool facingRight = true;
    private Vector2 gravityDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Disable default gravity since we're implementing our own
        rb.gravityScale = 0;

        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.parent = transform;
            check.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = check.transform;
            if (debugMode) Debug.Log("Ground check created. Please adjust its position in the inspector.");
        }

        // Create world center if it doesn't exist
        if (worldCenter == null)
        {
            GameObject center = GameObject.Find("WorldCenter");
            if (center == null)
            {
                center = new GameObject("WorldCenter");
                center.transform.position = Vector3.zero;
                if (debugMode) Debug.Log("World center created at origin. Adjust as needed.");
            }
            worldCenter = center.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Jump when on ground and jump button pressed
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (debugMode) Debug.Log("On ground and jump button pressed");
            // Jump in the opposite direction of gravity (away from center)
            rb.AddForce(-gravityDirection * jumpForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        // Calculate gravity direction (from player to world center)
        Vector2 playerToCenter = (Vector2)worldCenter.position - rb.position;
        gravityDirection = playerToCenter.normalized;

        // Calculate distance to center
        float distanceToCenter = playerToCenter.magnitude;

        // Apply radial gravity if inside the world boundary
        if (distanceToCenter < worldRadius)
        {
            // Apply gravity away from center
            rb.AddForce(-gravityDirection * gravityStrength);

            // Rotate player so feet point to circle edge
            float angle = Mathf.Atan2(playerToCenter.y, playerToCenter.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle + 90);

            // Check if player is grounded - updated for circular world
            // Calculate the distance from ground check to world center
            float groundCheckDistanceToCenter = ((Vector2)worldCenter.position - (Vector2)groundCheck.position).magnitude;

            // Get distance from groundCheck to the edge of the world
            float distanceToEdge = worldRadius - groundCheckDistanceToCenter;

            // Player is grounded if the feet are close to the edge of the world AND
            // there's ground layer detected (using a smaller check radius for precision)
            bool nearWorldEdge = distanceToEdge <= groundedDistanceThreshold;
            bool groundLayerDetected = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

            isGrounded = nearWorldEdge;

            // Optional debugging - only draw if debug mode is enabled
            if (debugMode)
            {
                Debug.DrawLine(groundCheck.position, worldCenter.position, isGrounded ? Color.green : Color.red);
            }

            // Calculate the tangent direction for movement along the circle
            // This is perpendicular to the gravity direction
            Vector2 tangent = new Vector2(-gravityDirection.y, gravityDirection.x);

            // Apply movement in the tangent direction with corrected directional logic
            Vector2 movementDirection = tangent * moveInput;

            // Keep vertical velocity component (jumping/falling)
            Vector2 verticalVelocity = Vector2.Dot(rb.linearVelocity, -gravityDirection) /
                                       Vector2.Dot(-gravityDirection, -gravityDirection) *
                                       (-gravityDirection);

            // Combine horizontal movement with vertical velocity
            rb.linearVelocity = movementDirection * moveSpeed + verticalVelocity;

            // Flip the player based on the input direction
            if (moveInput > 0 && !facingRight)
            {
                Flip();
            }
            else if (moveInput < 0 && facingRight)
            {
                Flip();
            }
        }
        else
        {
            // Push player back inside if they somehow get outside the world
            rb.linearVelocity = gravityDirection * moveSpeed;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        // We don't flip the scale since we're rotating the player
    }

    void OnDrawGizmos()
    {
        // Only draw gizmos if debug mode is enabled
        if (!debugMode) return;

        // Draw the world boundary for debugging
        if (worldCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(worldCenter.position, worldRadius);

            // Draw the ground check threshold
            if (groundCheck != null)
            {
                Gizmos.color = Color.yellow;
                Vector2 groundCheckToCenter = worldCenter.position - groundCheck.position;
                float groundCheckDistance = groundCheckToCenter.magnitude;
                if (groundCheckDistance > 0)
                {
                    Vector2 edgePosition = (Vector2)groundCheck.position +
                                          (groundCheckToCenter.normalized * (worldRadius - groundCheckDistance));
                    Gizmos.DrawLine(groundCheck.position, edgePosition);
                    Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
                }
            }
        }
    }
}
