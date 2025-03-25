using UnityEngine;

public class PlayerControllerNoRB : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float maxFallSpeed = 15f;
    [SerializeField] private float groundFriction = 0.8f;
    [SerializeField] private float airFriction = 0.95f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundedDistanceThreshold = 0.1f; // Distance threshold to be considered grounded
    [SerializeField] private LayerMask groundLayer;

    private bool isGrounded;
    private float moveInput;
    private bool facingRight = true;
    private Vector2 velocity;
    private Vector2 gravityDirection;
    private CircularWorldController worldController;

    // Start is called once before the first execution of Update
    void Start()
    {
        // Get reference to the world controller
        worldController = CircularWorldController.Instance;
        if (worldController == null)
        {
            Debug.LogError("CircularWorldController not found in the scene! Player won't move correctly.");
        }

        // Initialize velocity to zero
        velocity = Vector2.zero;

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

            // Calculate jump direction (opposite to gravity)
            Vector2 jumpDirection = -gravityDirection;

            // Apply jump force
            velocity += jumpDirection * jumpForce;
        }
    }

    void FixedUpdate()
    {
        if (worldController == null) return;

        // Calculate gravity direction (from player to world center)
        Vector2 playerToCenter = (Vector2)worldController.transform.position - (Vector2)transform.position;
        gravityDirection = playerToCenter.normalized;

        // Check if player is grounded
        bool wasGrounded = isGrounded;
        isGrounded = worldController.IsAtWorldEdge((Vector2)groundCheck.position, groundedDistanceThreshold);

        // Calculate the tangent direction for movement along the circle
        Vector2 tangent = worldController.GetTangentDirection(transform.position);

        // Calculate forces
        Vector2 gravityForce = worldController.CalculateGravity(transform.position) * gravityScale;
        Vector2 movementForce = -tangent * moveInput * moveSpeed;

        // If just landed, remove vertical velocity component
        if (isGrounded && !wasGrounded)
        {
            // Set vertical velocity to zero
            Vector2 normalDirection = gravityDirection;
            float verticalComponent = Vector2.Dot(velocity, normalDirection);
            if (verticalComponent > 0) // Only remove if moving toward ground
            {
                velocity -= verticalComponent * normalDirection;
            }
        }

        // Apply gravity when not grounded
        if (!isGrounded)
        {
            velocity += gravityForce * Time.fixedDeltaTime;
        }

        // Apply movement force to velocity
        velocity += movementForce * Time.fixedDeltaTime;

        // Apply friction
        float frictionFactor = isGrounded ? groundFriction : airFriction;
        velocity *= frictionFactor;

        // Limit fall speed
        float verticalSpeed = Vector2.Dot(velocity, -gravityDirection);
        if (verticalSpeed > maxFallSpeed)
        {
            velocity = velocity - (-gravityDirection * (verticalSpeed - maxFallSpeed));
        }

        // Calculate new position
        Vector2 newPosition = (Vector2)transform.position + velocity * Time.fixedDeltaTime;

        // Ensure player stays inside world
        newPosition = worldController.ConstrainToWorld(newPosition);

        // Update position
        transform.position = newPosition;

        // Rotate player so feet point to circle edge (away from center)
        float rotationAngle = worldController.GetAlignmentAngle(transform.position);
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle + 90f);

        // Optional debugging - only draw if debug mode is enabled
        if (debugMode)
        {
            Debug.DrawLine(groundCheck.position, worldController.transform.position, isGrounded ? Color.green : Color.red);
            Debug.DrawRay(transform.position, velocity, Color.blue);
            Debug.DrawRay(transform.position, movementForce, Color.yellow);
        }

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

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void OnDrawGizmos()
    {
        // Only draw gizmos if debug mode is enabled
        if (!debugMode) return;

        // Draw the ground check threshold
        if (groundCheck != null && CircularWorldController.Instance != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}
