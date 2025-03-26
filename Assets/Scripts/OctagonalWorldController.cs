using UnityEngine;

public class OctagonalWorldController : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;

    [Header("Octagonal World")]
    [SerializeField] private float worldRadius = 10f;
    [SerializeField] private float gravityStrength = 20f;
    [SerializeField] private string boundaryTag = "Boundary";
    [SerializeField] private LayerMask boundaryLayer;
    
    [Header("Boundary Object")]
    [SerializeField] private GameObject boundaryPrefab; // The prefab to be used for boundaries
    [SerializeField] private float rectangleWidth = 30f; // Width of boundary rectangle
    [SerializeField] private float rectangleHeight = 100f; // Height of boundary rectangle

    // Rectangles that form the octagon
    private GameObject[] boundaryRectangles;
    private const int SEGMENTS = 8;

    // Singleton instance
    private static OctagonalWorldController _instance;
    public static OctagonalWorldController Instance { get { return _instance; } }

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        if (boundaryPrefab == null)
        {
            Debug.LogError("Boundary prefab is not assigned! Please assign a GameObject with SpriteRenderer and BoxCollider2D.");
            return;
        }
        
        GenerateOctagonalWorld();
    }

    /// <summary>
    /// Generate the octagonal world using 8 rectangles
    /// </summary>
    private void GenerateOctagonalWorld()
    {
        // Clean up any existing boundaries
        if (boundaryRectangles != null)
        {
            foreach (var rectangle in boundaryRectangles)
            {
                if (rectangle != null)
                    Destroy(rectangle);
            }
        }

        // Create a parent object for all boundaries
        GameObject boundariesParent = new GameObject("OctagonBoundaries");
        boundariesParent.transform.parent = transform;
        boundariesParent.transform.localPosition = Vector3.zero;

        // Initialize array
        boundaryRectangles = new GameObject[SEGMENTS];
        
        // Create 8 rectangles to form the octagon
        for (int i = 0; i < SEGMENTS; i++)
        {
            // Calculate angle for this segment
            float angle = i * (360f / SEGMENTS);
            float radians = angle * Mathf.Deg2Rad;

            // Instantiate the boundary prefab
            boundaryRectangles[i] = Instantiate(boundaryPrefab, boundariesParent.transform);
            boundaryRectangles[i].name = $"Boundary_{i}";
            boundaryRectangles[i].tag = boundaryTag;
            
            // Set layer if needed
            if (boundaryLayer != 0)
            {
                boundaryRectangles[i].layer = Mathf.RoundToInt(Mathf.Log(boundaryLayer.value, 2));
            }

            // Calculate the octagon radius at this angle to ensure alignment with gizmos
            float octagonRadius = GetOctagonRadiusAtAngle(radians);
            
            // Calculate the correct position for the rectangle
            // Place it so its inner edge aligns with the octagon edge
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            
            // Position the rectangle so that its inner edge touches the octagon edge
            // Add half of rectangle height to push it outward from the octagon edge
            Vector3 position = transform.position + new Vector3(
                direction.x * (octagonRadius + rectangleHeight/2),
                direction.y * (octagonRadius + rectangleHeight/2),
                0);
            
            // Set position and rotation
            boundaryRectangles[i].transform.position = position;
            boundaryRectangles[i].transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // Set scale based on the desired rectangle dimensions
            boundaryRectangles[i].transform.localScale = new Vector3(rectangleWidth, rectangleHeight, 1);
        }
    }

    /// <summary>
    /// Calculate gravity vector for an object at the given position
    /// </summary>
    /// <param name="position">The object's position</param>
    /// <returns>Gravity force vector to apply</returns>
    public Vector2 CalculateGravity(Vector2 position)
    {
        if (position == Vector2.zero)
        {
            position = new Vector2(0f, -0.1f);
        }

        // Calculate gravity direction (from object to world center)
        Vector2 toCenter = (Vector2)transform.position - position;
        
        // Get closest edge point to determine accurate gravity direction
        Vector2 closestEdgePoint = GetClosestEdgePoint(position);
        Vector2 gravityDirection = ((Vector2)transform.position - closestEdgePoint).normalized;

        // Calculate distance to center
        float distanceToCenter = toCenter.magnitude;

        // Calculate gravity strength based on distance from edge
        float distanceToEdge = worldRadius - distanceToCenter;
        float gravityMultiplier = Mathf.Clamp01(distanceToEdge / 0.5f); // Reduces gravity near edge

        // Return the gravity force (negative to push away from center)
        return -gravityDirection * gravityStrength * gravityMultiplier;
    }

    /// <summary>
    /// Check if position is inside the world
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <returns>True if inside or at boundary</returns>
    public bool IsInsideWorld(Vector2 position)
    {
        Vector2 localPos = position - (Vector2)transform.position;
        
        // For octagon, we check if it's within the world radius at the appropriate angle
        float angle = Mathf.Atan2(localPos.y, localPos.x);
        float octagonRadius = GetOctagonRadiusAtAngle(angle);
        
        return localPos.magnitude <= octagonRadius;
    }

    /// <summary>
    /// Push a position back inside the world if it's outside
    /// </summary>
    /// <param name="position">Current position</param>
    /// <returns>Corrected position</returns>
    public Vector2 ConstrainToWorld(Vector2 position)
    {
        Vector2 toCenter = (Vector2)transform.position - position;
        float angle = Mathf.Atan2(toCenter.y, toCenter.x);
        float octagonRadius = GetOctagonRadiusAtAngle(angle);

        if (toCenter.magnitude > octagonRadius)
        {
            // If outside, push back to the boundary
            return (Vector2)transform.position - (toCenter.normalized * octagonRadius);
        }

        return position;
    }

    /// <summary>
    /// Check if an object is at the edge of the world
    /// </summary>
    /// <param name="checkPosition">Position to check</param>
    /// <param name="threshold">Distance threshold to be considered at the edge</param>
    /// <returns>True if the position is within threshold distance of the world edge</returns>
    public bool IsAtWorldEdge(Vector2 checkPosition, float threshold)
    {
        Vector2 toCenter = (Vector2)transform.position - checkPosition;
        float angle = Mathf.Atan2(toCenter.y, toCenter.x);
        float octagonRadius = GetOctagonRadiusAtAngle(angle);

        // Get distance from check position to the edge of the world
        float distanceToEdge = octagonRadius - toCenter.magnitude;

        // Position is at edge if it's close enough to the edge
        return distanceToEdge <= threshold;
    }

    /// <summary>
    /// Get rotation angle to align an object with the world's center
    /// </summary>
    /// <param name="position">The position of the object to align</param>
    /// <returns>The rotation in Euler angles (Z axis) - points toward center</returns>
    public float GetAlignmentAngle(Vector2 position)
    {
        Vector2 closestEdgePoint = GetClosestEdgePoint(position);
        Vector2 toCenter = (Vector2)transform.position - closestEdgePoint;
        return Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg + 180f;
    }

    /// <summary>
    /// Get the direction of gravity for an object at the given position
    /// </summary>
    /// <param name="position">The position to calculate gravity for</param>
    /// <returns>The gravity vector (normalized)</returns>
    public Vector2 GetGravityDirection(Vector2 position)
    {
        Vector2 closestEdgePoint = GetClosestEdgePoint(position);
        Vector2 toCenter = (Vector2)transform.position - closestEdgePoint;
        return toCenter.normalized;
    }

    /// <summary>
    /// Get tangent direction for movement along the octagonal world
    /// This is the perpendicular to the direction toward the center
    /// </summary>
    /// <param name="position">The position to calculate tangent for</param>
    /// <returns>The tangent vector (normalized)</returns>
    public Vector2 GetTangentDirection(Vector2 position)
    {
        Vector2 gravityDir = GetGravityDirection(position);
        return new Vector2(-gravityDir.y, gravityDir.x);
    }

    /// <summary>
    /// Get the world radius
    /// </summary>
    public float WorldRadius { get { return worldRadius; } }

    /// <summary>
    /// Get a point on the octagonal world at the specified angle and distance from center
    /// </summary>
    /// <param name="angle">Angle in degrees (0 = right, 90 = up)</param>
    /// <param name="distance">Distance from center</param>
    /// <returns>Point in world space</returns>
    public Vector2 GetPointOnCircle(float angle, float distance)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        
        // Cap distance by octagon radius at this angle if needed
        float maxDistance = GetOctagonRadiusAtAngle(radians);
        distance = Mathf.Min(distance, maxDistance);
        
        return (Vector2)transform.position + direction * distance;
    }

    /// <summary>
    /// Align a point to the octagonal contour
    /// </summary>
    /// <param name="point">The point to align</param>
    /// <param name="radius">The radius factor (1.0 = edge of octagon)</param>
    /// <returns>The aligned point</returns>
    public Vector2 AlignPointToCircle(Vector2 point, float radius)
    {
        Vector2 toPoint = point - (Vector2)transform.position;
        float angle = Mathf.Atan2(toPoint.y, toPoint.x);
        float octagonRadius = GetOctagonRadiusAtAngle(angle) * radius;
        return (Vector2)transform.position + toPoint.normalized * octagonRadius;
    }

    /// <summary>
    /// Get the octagon radius at a specific angle
    /// </summary>
    /// <param name="angle">Angle in radians</param>
    /// <returns>Radius at the specified angle</returns>
    private float GetOctagonRadiusAtAngle(float angle)
    {
        // Normalize angle to 0-2π
        angle = (angle + 2 * Mathf.PI) % (2 * Mathf.PI);
        
        // For an octagon, we need to find which segment we're in
        float segmentAngle = 2 * Mathf.PI / SEGMENTS;
        int segment = Mathf.FloorToInt(angle / segmentAngle);
        
        // Calculate angle within the segment
        float angleInSegment = angle - segment * segmentAngle;
        
        // Adjust to get angle from segment center
        float angleFromSegmentCenter = Mathf.Abs(angleInSegment - segmentAngle / 2);
        
        // For regular octagon: radius = s / (2 * tan(π/8))
        // where s is the side length
        float innerRadius = worldRadius * Mathf.Cos(Mathf.PI / SEGMENTS);
        float radiusAtAngle = innerRadius / Mathf.Cos(angleFromSegmentCenter);
        
        return radiusAtAngle;
    }

    /// <summary>
    /// Get the closest point on the octagon edge to the given position
    /// </summary>
    /// <param name="position">The position to check from</param>
    /// <returns>The closest point on the octagon edge</returns>
    private Vector2 GetClosestEdgePoint(Vector2 position)
    {
        Vector2 toPoint = position - (Vector2)transform.position;
        float angle = Mathf.Atan2(toPoint.y, toPoint.x);
        float octagonRadius = GetOctagonRadiusAtAngle(angle);
        
        return (Vector2)transform.position + toPoint.normalized * octagonRadius;
    }

    /// <summary>
    /// Update the world radius and regenerate the world if needed
    /// </summary>
    /// <param name="newRadius">New radius value</param>
    public void UpdateWorldRadius(float newRadius)
    {
        worldRadius = newRadius;
        GenerateOctagonalWorld();
    }
    
    /// <summary>
    /// Update the boundary dimensions and regenerate the world
    /// </summary>
    /// <param name="width">New rectangle width</param>
    /// <param name="height">New rectangle height</param>
    public void UpdateBoundaryDimensions(float width, float height)
    {
        rectangleWidth = width;
        rectangleHeight = height;
        GenerateOctagonalWorld();
    }

    private void OnDrawGizmos()
    {
        if (!debugMode) return;

        // Draw octagon boundary
        Gizmos.color = Color.green;
        
        // Draw lines between points on the octagon
        for (int i = 0; i < SEGMENTS; i++)
        {
            // Use the same angle calculation as in the GenerateOctagonalWorld method
            float angle1 = i * (360f / SEGMENTS) * Mathf.Deg2Rad;
            float angle2 = ((i + 1) % SEGMENTS) * (360f / SEGMENTS) * Mathf.Deg2Rad;
            
            // Calculate points on the octagon edge using the same radius calculation
            Vector2 toPoint1 = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1));
            Vector2 toPoint2 = new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2));
            
            // Use the octagon radius calculation to get correct points
            float octagonRadius1 = GetOctagonRadiusAtAngle(angle1);
            float octagonRadius2 = GetOctagonRadiusAtAngle(angle2);
            
            Vector3 point1 = transform.position + new Vector3(
                toPoint1.x * octagonRadius1,
                toPoint1.y * octagonRadius1,
                0);
                
            Vector3 point2 = transform.position + new Vector3(
                toPoint2.x * octagonRadius2,
                toPoint2.y * octagonRadius2,
                0);
                
            Gizmos.DrawLine(point1, point2);
        }
    }
}
