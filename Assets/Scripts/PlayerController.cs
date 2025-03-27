using UnityEngine;

public class PlayerController : MonoBehaviour
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
    [SerializeField] private float fallAcceleration = 1.5f; // Additional acceleration when falling

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundedDistanceThreshold = 0.1f; // Distance threshold to be considered grounded
    [SerializeField] private LayerMask groundLayer;

    [Header("Air Movement")]
    [SerializeField] private float airRotationSpeedReduction = 0.8f; // How much speed is reduced when changing direction in air

    [Header("Platform")]
    [SerializeField] private float platformSnapDistance = 0.2f;
    [SerializeField] private bool snapToPlatforms = true;

    private bool isGrounded;
    private float moveInput;
    private bool facingRight = true;
    private Vector2 velocity;
    private Vector2 gravityDirection;
    private CircularWorldController worldController;
    private float savedHorizontalSpeed = 0f;
    private int lastAirDirection = 0; // 0 = neutral, -1 = left, 1 = right
    private bool wasGrounded; // Track previous grounded state

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
        ProcessInput();
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        if (worldController == null) return;

        UpdateGravityDirection();
        CheckGroundState();
        CalculateMovementForces();
        ApplyPhysics();
        UpdatePlayerPosition();
        UpdatePlayerRotation();
        HandlePlayerFlip();
        DrawDebugVisuals();
    }

    // Process player input
    private void ProcessInput()
    {
        // Get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    // Handle jump input and perform jump if conditions are met
    private void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (debugMode) Debug.Log("On ground and jump button pressed");
            
            // Apply jump force in the opposite direction of gravity
            velocity += gravityDirection * jumpForce;
        }
    }
    
    // Update the gravity direction based on player position
    private void UpdateGravityDirection()
    {
        gravityDirection = worldController.GetGravityDirection(transform.position);
    }
    
    private void CheckGroundState()
    {
        wasGrounded = isGrounded;
        
        // Check if we're on the world edge
        bool onWorldEdge = worldController.IsAtWorldEdge((Vector2)groundCheck.position, groundedDistanceThreshold);
        
        // Check for platforms - cast from slightly below feet up 
        Vector2 rayOrigin = (Vector2)groundCheck.position + gravityDirection * platformSnapDistance * 0.5f;
        Vector2 rayDirection = -gravityDirection; // Cast opposite to gravity (up)
        
        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin, 
            rayDirection, 
            platformSnapDistance, 
            groundLayer
        );
        
        bool onPlatform = hit.collider != null;
        
        // Player is grounded if either condition is true
        isGrounded = onWorldEdge || onPlatform;
        
        // Handle transition states
        if (wasGrounded && !isGrounded)
        {
            HandleLeavingGround();
        }
        else if (isGrounded && !wasGrounded)
        {
            HandleLanding();
        }
        
        if (debugMode && isGrounded)
        {
            string platformName = onPlatform ? hit.collider.gameObject.name : "none";
            Debug.DrawRay(rayOrigin, rayDirection * platformSnapDistance, Color.cyan, 0.1f);
            Debug.Log($"Grounded: World Edge={onWorldEdge}, Platform={onPlatform} ({platformName})");
        }
    }
    
    // Handle player leaving the ground (jumping or falling)
    private void HandleLeavingGround()
    {
        Vector2 tangent = worldController.GetTangentDirection(transform.position);
        savedHorizontalSpeed = Mathf.Abs(Vector2.Dot(velocity, -tangent)) * Mathf.Sign(Vector2.Dot(velocity, -tangent));
        lastAirDirection = (int)Mathf.Sign(moveInput);
        
        if (debugMode) Debug.Log($"Left ground, saved speed: {savedHorizontalSpeed}");
    }
    
    // Handle player landing on the ground
    private void HandleLanding()
    {
        // Set vertical velocity to zero
        Vector2 normalDirection = gravityDirection;
        float verticalComponent = Vector2.Dot(velocity, normalDirection);
        if (verticalComponent > 0) // Only remove if moving toward ground
        {
            velocity -= verticalComponent * normalDirection;
        }
        
        // Transfer any air speed to regular velocity
        Vector2 tangent = worldController.GetTangentDirection(transform.position);
        float horizontalComponent = Vector2.Dot(velocity, -tangent);
        velocity = -tangent * horizontalComponent;
    }
    
    // Calculate forces for movement
    private void CalculateMovementForces()
    {
        Vector2 tangent = worldController.GetTangentDirection(transform.position);
        Vector2 movementForce = Vector2.zero;
        
        if (isGrounded)
        {
            // When grounded, apply normal movement force
            movementForce = -tangent * moveInput * moveSpeed;
            
            // Reset air movement variables when safely on ground
            if (wasGrounded)
            {
                savedHorizontalSpeed = 0f;
                lastAirDirection = 0;
            }
        }
        else
        {
            // When in air, check if player changed direction
            int currentAirDirection = (int)Mathf.Sign(moveInput);
            
            if (currentAirDirection != 0 && lastAirDirection != 0 && currentAirDirection != lastAirDirection)
            {
                // Player is trying to change direction in air
                // Invert the horizontal speed direction and apply reduction
                savedHorizontalSpeed = -savedHorizontalSpeed * airRotationSpeedReduction;
                lastAirDirection = currentAirDirection;
                
                if (debugMode) Debug.Log($"Direction change in air, speed inverted to: {savedHorizontalSpeed}");
            }
            
            // Apply the saved horizontal speed
            movementForce = -tangent * savedHorizontalSpeed;
        }
        
        // Add movement force to velocity
        velocity += movementForce * Time.fixedDeltaTime;
    }
    
    // Apply physics calculations (gravity, friction, speed limits)
    private void ApplyPhysics()
    {
        // Apply gravity when not grounded
        if (!isGrounded)
        {
            Vector2 gravityForce = worldController.CalculateGravity(transform.position) * gravityScale;
            
            // Check if player is falling (moving in direction of gravity)
            float verticalVelocity = Vector2.Dot(velocity, -gravityDirection);
            if (verticalVelocity > 0)
            {
                // Player is falling, apply additional downward acceleration
                gravityForce *= fallAcceleration;
            }
            
            velocity += gravityForce * Time.fixedDeltaTime;
        }

        // Apply friction
        float frictionFactor = isGrounded ? groundFriction : airFriction;
        velocity *= frictionFactor;

        // Limit fall speed
        float verticalSpeed = Vector2.Dot(velocity, -gravityDirection);
        if (verticalSpeed > maxFallSpeed)
        {
            velocity = velocity - (-gravityDirection * (verticalSpeed - maxFallSpeed));
        }
    }
    
    private void UpdatePlayerPosition()
    {
        // Calculate new position
        Vector2 newPosition = (Vector2)transform.position + velocity * Time.fixedDeltaTime;

        // Ensure player stays inside world
        newPosition = worldController.ConstrainToWorld(newPosition);

        // Check if we need to snap to curved platforms
        if (isGrounded && snapToPlatforms)
        {
            // Cast a ray up from below the player's feet
            // This ensures we hit the top of platforms
            Vector2 rayOrigin = newPosition + gravityDirection * platformSnapDistance;
            Vector2 rayDirection = -gravityDirection;
            
            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin, 
                rayDirection, 
                platformSnapDistance * 2f, // Increased distance to ensure we hit the platform
                groundLayer
            );

            if (hit.collider != null)
            {
                // Adjust the position to stay on the platform surface, offset enough to avoid noclips
                newPosition = hit.point + (rayDirection * groundedDistanceThreshold);
                
                if (debugMode)
                {
                    Debug.DrawLine(rayOrigin, hit.point, Color.magenta, 0.1f);
                    Debug.Log($"Snapping to platform at {hit.point}");
                }
            }
            else
            {
                // If we're not over a platform, check if we're on the world edge
                bool onWorldEdge = worldController.IsAtWorldEdge(newPosition, groundedDistanceThreshold);
                if (!onWorldEdge)
                {
                    isGrounded = false;
                }
            }
        }

        transform.position = newPosition;
    }
    
    // Update player rotation to align with world
    private void UpdatePlayerRotation()
    {
        // Rotate player so feet point to circle edge (away from center)
        float rotationAngle = worldController.GetAlignmentAngle(transform.position);
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle + 90f);
    }
    
    // Handle flipping the player sprite based on movement direction
    private void HandlePlayerFlip()
    {
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }
    }
    
    // Draw debug visuals if debug mode is enabled
    private void DrawDebugVisuals()
    {
        if (!debugMode) return;
        
        Debug.DrawLine(groundCheck.position, worldController.transform.position, isGrounded ? Color.green : Color.red);
        Debug.DrawRay(transform.position, velocity, Color.blue);
        
        // Draw movement force (approximated since we don't have the actual force variable here)
        Vector2 tangent = worldController.GetTangentDirection(transform.position);
        Vector2 approximatedForce = -tangent * (isGrounded ? moveInput * moveSpeed : savedHorizontalSpeed);
        Debug.DrawRay(transform.position, approximatedForce, Color.yellow);
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
