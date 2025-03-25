using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

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
            Debug.Log("Ground check created. Please adjust its position in the inspector.");
        }

        // Create world center if it doesn't exist
        if (worldCenter == null)
        {
            GameObject center = GameObject.Find("WorldCenter");
            if (center == null)
            {
                center = new GameObject("WorldCenter");
                center.transform.position = Vector3.zero;
                Debug.Log("World center created at origin. Adjust as needed.");
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

            // Check if player is grounded
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

            // Calculate the tangent direction for movement along the circle
            // This is perpendicular to the gravity direction
            Vector2 tangent = new Vector2(-gravityDirection.y, gravityDirection.x);

            // Apply movement in the tangent direction with corrected directional logic
            // Right movement (moveInput > 0) should move clockwise
            // Left movement (moveInput < 0) should move counter-clockwise
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
        // Draw the world boundary for debugging
        if (worldCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(worldCenter.position, worldRadius);
        }
    }
}
