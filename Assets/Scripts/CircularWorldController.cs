using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class CircularWorldController : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;

    [Header("Circular World")]
    [SerializeField] private float worldRadius = 10f;
    [SerializeField] private float gravityStrength = 20f;

    // Singleton instance
    private static CircularWorldController _instance;
    public static CircularWorldController Instance { get { return _instance; } }

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
        Vector2 gravityDirection = toCenter.normalized;

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
        float distanceToCenter = ((Vector2)transform.position - position).magnitude;
        return distanceToCenter <= worldRadius;
    }

    /// <summary>
    /// Push a position back inside the world if it's outside
    /// </summary>
    /// <param name="position">Current position</param>
    /// <returns>Corrected position</returns>
    public Vector2 ConstrainToWorld(Vector2 position)
    {
        Vector2 toCenter = (Vector2)transform.position - position;
        float distanceToCenter = toCenter.magnitude;

        if (distanceToCenter > worldRadius)
        {
            // If outside, push back to the boundary
            return (Vector2)transform.position - (toCenter.normalized * worldRadius);
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
        // Calculate distance from check position to world center
        float distanceToCenter = ((Vector2)transform.position - checkPosition).magnitude;

        // Get distance from check position to the edge of the world
        float distanceToEdge = worldRadius - distanceToCenter;

        // Position is at edge if it's close enough to the edge
        return distanceToEdge <= threshold;
    }

    /// <summary>
    /// Get rotation angle to align an object with the world's center
    /// Make sure the toes point out
    /// </summary>
    /// <param name="position">The position of the object to align</param>
    /// <returns>The rotation in Euler angles (Z axis) - points toward center</returns>
    public float GetAlignmentAngle(Vector2 position)
    {
        Vector2 toCenter = (Vector2)transform.position - position;
        return Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg + 180f;
    }


    /// <summary>
    /// Get the direction of gravity for an object at the given position
    /// </summary>
    /// <param name="position">The position to calculate gravity for</param>
    /// <returns>The gravity vector (normalized)</returns>
    public Vector2 GetGravityDirection(Vector2 position)
    {
        Vector2 toCenter = (Vector2)transform.position - position;
        return toCenter.normalized;
    }

    /// <summary>
    /// Get tangent direction for movement along the circular world
    /// This is the perpendicular to the direction toward the center
    /// </summary>
    /// <param name="position">The position to calculate tangent for</param>
    /// <returns>The tangent vector (normalized)</returns>
    public Vector2 GetTangentDirection(Vector2 position)
    {
        Vector2 toCenter = (Vector2)transform.position - position;
        Vector2 gravityDir = toCenter.normalized;
        return new Vector2(-gravityDir.y, gravityDir.x);
    }

    /// <summary>
    /// Get the world radius
    /// </summary>
    public float WorldRadius { get { return worldRadius; } }

    /// <summary>
    /// Get a point on the circular world at the specified angle and distance from center
    /// </summary>
    /// <param name="angle">Angle in degrees (0 = right, 90 = up)</param>
    /// <param name="distance">Distance from center</param>
    /// <returns>Point in world space</returns>
    public Vector2 GetPointOnCircle(float angle, float distance)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        return (Vector2)transform.position + direction * distance;
    }

    /// <summary>
    /// Align a point to the circular contour at a specific radius
    /// </summary>
    /// <param name="point">The point to align</param>
    /// <param name="radius">The radius to align to</param>
    /// <returns>The aligned point</returns>
    public Vector2 AlignPointToCircle(Vector2 point, float radius)
    {
        Vector2 toPoint = point - (Vector2)transform.position;
        float angle = Mathf.Atan2(toPoint.y, toPoint.x);
        return GetPointOnCircle(angle * Mathf.Rad2Deg, radius);
    }

    private void OnDrawGizmos()
    {
        if (!debugMode) return;

        // Draw world boundary and background
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, worldRadius);
    }
}
