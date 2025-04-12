using UnityEngine;

public class OrbitAroundPlayer : MonoBehaviour
{
    [SerializeField] public Transform player;
    [SerializeField] public float radius = 1.5f;
    [SerializeField] public float orbitSpeed = 2f;

    [SerializeField] private float currentAngle = 0f;

    void Start()
    {
        
    }

    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f; // Ensure it's in 2D plane

        // Step 2: Get the direction from the player to the mouse
        Vector3 dirToMouse = (mouseWorldPos - player.position).normalized;



        Vector2 orbitCenter = player.position + dirToMouse * radius;
        // Get direction from the player to the mouse
        Vector2 direction = (mouseWorldPos - player.position).normalized;
        // Get angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotate the gun around the playerfloat offsetX = Mathf.Cos(angle) * radius;
        float offsetY = Mathf.Sin(angle) * radius;
        float offsetX = Mathf.Cos(angle) * radius;
        Vector3 orbitOffset = new Vector3(offsetX, offsetY, 0f);

        // Apply position around the player, influenced by mouse direction
        transform.position = player.position + Quaternion.Euler(0, 0, Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg) * orbitOffset;
    
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
