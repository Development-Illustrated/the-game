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
    /// </summary>
    /// <param name="position">The position of the object to align</param>
    /// <returns>The rotation in Euler angles (Z axis) - points toward center</returns>
    public float GetAlignmentAngle(Vector2 position)
    {
        Vector2 toCenter = (Vector2)transform.position - position;
        return Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg + 180f;
    }

    /// <summary>
    /// Get tangent direction for movement along the circular world
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

    private void OnDrawGizmos()
    {
        // Only draw gizmos if debug mode is enabled
        if (!debugMode) return;

        // Draw the world boundary
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, worldRadius);
    }
}
