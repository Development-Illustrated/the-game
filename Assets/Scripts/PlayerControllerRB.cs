using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerRB : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float acceleration = 20f;   // Replaces moveSpeed
    [SerializeField] private float maxSpeed = 7f;        // Maximum allowed speed
    [SerializeField] private float deceleration = 30f;   // How quickly player slows down when not moving
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Player Settings")]
    [SerializeField] private bool facingRight = true;    // Initial facing direction
    [SerializeField] private GameObject graphicsObject; // Reference to the player graphics object

    // Components
    private Rigidbody2D rb;
    private CircularWorldController worldController;

    // State tracking
    private bool isGrounded = false;
    private float moveInput = 0f;
    private bool jumpRequested = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // If graphicsObject wasn't assigned in inspector, try to find a child object
        if (graphicsObject == null)
        {
            // Look for a child named "Graphics" or similar
            Transform graphics = transform.Find("Graphics");
            if (graphics != null)
            {
                graphicsObject = graphics.gameObject;
            }
        }
    }

    private void Start()
    {
        // Configure rigidbody
        rb.gravityScale = 0f; // We'll handle gravity manually
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation from physics

        // Get reference to world controller
        worldController = CircularWorldController.Instance;

    }

    private void Update()
    {
        // Get input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Jump input
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }

        // Check if player is grounded
        CheckGrounded();
    }

    private void FixedUpdate()
    {
        // Apply gravity
        ApplyGravity();

        // Move player
        MovePlayer();

        // Handle jump
        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }

        // Align rotation with world
        AlignWithWorld();

        // Constrain position to world bounds
        ConstrainToWorld();

    }

    private void ApplyGravity()
    {
        // Get gravity force from world controller
        Vector2 gravity = worldController.CalculateGravity(rb.position);
        rb.AddForce(gravity, ForceMode2D.Force);
    }

    private void MovePlayer()
    {
        // Get tangent direction for movement along the circular world
        Vector2 tangentDirection = worldController.GetTangentDirection(rb.position);

        // Get the player's current velocity
        Vector2 velocity = rb.linearVelocity;
        float currentTangentSpeed = 0;

        // Project velocity onto the tangent direction to get current speed along the circle
        if (moveInput > 0)
        {
            // For right movement, the tangent is negated
            currentTangentSpeed = -Vector2.Dot(velocity, tangentDirection);
        }
        else
        {
            currentTangentSpeed = Vector2.Dot(velocity, tangentDirection);
        }

        if (moveInput != 0)
        {
            // If we're below max speed, accelerate
            if (Mathf.Abs(currentTangentSpeed) < maxSpeed)
            {
                // If moving right, invert the tangent
                Vector2 forceDirection;
                if (moveInput > 0)
                {
                    forceDirection = -tangentDirection;
                    if (!facingRight) FlipSprite();
                }
                else // moving left
                {
                    forceDirection = tangentDirection;
                    if (facingRight) FlipSprite();
                }

                // Apply acceleration force
                rb.AddForce(forceDirection * acceleration, ForceMode2D.Force);
            }
        }
        else
        {
            // Player is not pressing movement keys, apply deceleration
            ApplyDeceleration(tangentDirection);
        }

        // Cap speed if we're going too fast
        LimitMaxSpeed();
    }

    /// <summary>
    /// Slows down the player when not pressing movement keys
    /// </summary>
    private void ApplyDeceleration(Vector2 tangentDirection)
    {
        // Get current velocity
        Vector2 velocity = rb.linearVelocity;

        // Calculate how much of our velocity is along the tangent
        float speedAlongTangent = Vector2.Dot(velocity, tangentDirection);
        float speedAlongNegativeTangent = Vector2.Dot(velocity, -tangentDirection);

        // Create deceleration force in the opposite direction of movement
        Vector2 decelerationForce;

        if (Mathf.Abs(speedAlongTangent) > Mathf.Abs(speedAlongNegativeTangent))
        {
            // Moving more along the positive tangent
            if (Mathf.Abs(speedAlongTangent) > 0.1f)
            {
                decelerationForce = -tangentDirection * Mathf.Sign(speedAlongTangent) * deceleration;
                rb.AddForce(decelerationForce, ForceMode2D.Force);
            }
        }
        else
        {
            // Moving more along the negative tangent
            if (Mathf.Abs(speedAlongNegativeTangent) > 0.1f)
            {
                decelerationForce = tangentDirection * Mathf.Sign(speedAlongNegativeTangent) * deceleration;
                rb.AddForce(decelerationForce, ForceMode2D.Force);
            }
        }
    }

    /// <summary>
    /// Limits the player's speed to maxSpeed
    /// </summary>
    private void LimitMaxSpeed()
    {
        // Get current velocity
        Vector2 velocity = rb.linearVelocity;

        // Get tangent direction
        Vector2 tangentDirection = worldController.GetTangentDirection(rb.position);

        // Calculate speed along the tangent
        float speedAlongPositiveTangent = Vector2.Dot(velocity, tangentDirection);
        float speedAlongNegativeTangent = Vector2.Dot(velocity, -tangentDirection);

        // Reconstruct velocity
        Vector2 tangentialVelocity = tangentDirection * speedAlongPositiveTangent +
                                    (-tangentDirection) * speedAlongNegativeTangent;

        // Get radial velocity (perpendicular to the circle at the player's position)
        Vector2 radialVelocity = velocity - tangentialVelocity;

        // Check if we need to cap the tangential speed
        float tangentialSpeed = tangentialVelocity.magnitude;
        if (tangentialSpeed > maxSpeed)
        {
            // Scale down tangential velocity to max speed
            tangentialVelocity = tangentialVelocity.normalized * maxSpeed;

            // Reconstruct velocity with capped tangential component
            rb.linearVelocity = radialVelocity + tangentialVelocity;
        }
    }

    /// <summary>
    /// Flips the graphics object horizontally and updates facing direction
    /// </summary>
    private void FlipSprite()
    {
        facingRight = !facingRight;

        if (graphicsObject != null)
        {
            // Flip by inverting the x scale
            Vector3 scale = graphicsObject.transform.localScale;
            scale.x *= -1;
            graphicsObject.transform.localScale = scale;
        }
    }

    private void Jump()
    {
        // Get direction away from center (normalized gravity direction)
        Vector2 jumpDirection = -worldController.CalculateGravity(rb.position).normalized;

        // Apply jump force
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);

        // Reset grounded state
        isGrounded = false;
    }

    private void CheckGrounded()
    {
        // Get direction toward center
        Vector2 toCenter = ((Vector2)worldController.transform.position - rb.position).normalized;

        // Cast ray toward center to check for ground
        RaycastHit2D hit = Physics2D.Raycast(rb.position, toCenter, groundCheckDistance, groundLayer);

        // Set grounded state
        isGrounded = hit.collider != null;

        // Alternative grounding check based on distance to world edge
        if (!isGrounded)
        {
            float distanceToCenter = Vector2.Distance(worldController.transform.position, rb.position);
            float distanceToEdge = worldController.WorldRadius - distanceToCenter;

            isGrounded = distanceToEdge <= groundCheckDistance;
        }
    }

    private void AlignWithWorld()
    {
        // Get alignment angle from world controller
        float targetAngle = worldController.GetAlignmentAngle(rb.position);

        // Add a 90-degree offset to fix the rotation (player faces correct direction)
        targetAngle += 90f;

        // Calculate current rotation and target rotation
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        // Smoothly rotate to align with world
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void ConstrainToWorld()
    {
        // Calculate distance to center
        Vector2 toCenter = (Vector2)worldController.transform.position - rb.position;
        float distanceToCenter = toCenter.magnitude;

        // Get normalized direction to center
        Vector2 toCenterDir = toCenter.normalized;

        if (distanceToCenter > worldController.WorldRadius)
        {
            // If outside, push back to the adjusted boundary
            rb.position = (Vector2)worldController.transform.position - (toCenterDir * worldController.WorldRadius);

            // Adjust velocity to prevent drifting away
            // Project velocity onto the tangent plane to the world boundary
            Vector2 velocity = rb.linearVelocity;
            float inwardVelocity = Vector2.Dot(velocity, toCenterDir);

            // If moving outward (away from center), cancel that component of velocity
            if (inwardVelocity < 0)
            {
                Vector2 outwardVelocity = toCenterDir * inwardVelocity;
                rb.linearVelocity -= outwardVelocity;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw ground check ray
        if (worldController != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector2 toCenter = ((Vector2)worldController.transform.position - (Vector2)transform.position).normalized;
            Gizmos.DrawRay(transform.position, toCenter * groundCheckDistance);
        }
    }
}
