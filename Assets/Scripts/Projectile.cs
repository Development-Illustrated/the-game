using UnityEngine;

[RequireComponent(typeof(Hit))]
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileData data;
    [SerializeField] private bool debugMode = false;

    private Rigidbody2D rb;
    private Hit hitComponent;
    private float lifeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitComponent = GetComponent<Hit>();
    }

    private void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[Projectile:{gameObject.name}] ProjectileData is not assigned!");
            Destroy(gameObject);
            return;
        }

        // Configure the Hit component based on projectile data
        hitComponent.Configure(
            data.damage,
            data.destroyOnHit,
            data.hitMaxTimes,
            data.hitCooldown
        );

        // Apply initial velocity
        rb.linearVelocity = transform.right * data.velocity;

        // Apply gravity scale
        rb.gravityScale = data.gravityScale;

        lifeTimer = 0f;
    }

    private void Update()
    {
        // Handle lifetime
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= data.lifetime)
        {
            if (debugMode)
                Debug.Log($"[Projectile:{gameObject.name}] Destroyed due to lifetime expiration");

            Destroy(gameObject);
        }
    }

    public void Launch(Vector2 direction, float speedMultiplier = 1f)
    {
        // Normalize direction and apply velocity
        direction = direction.normalized;
        transform.right = direction; // Face the direction of movement
        rb.linearVelocity = direction * data.velocity * speedMultiplier;

        if (debugMode)
            Debug.Log($"[Projectile:{gameObject.name}] Launched with velocity={rb.linearVelocity}, direction={direction}");
    }
    public void SetProjectileData(ProjectileData newData)
    {
        data = newData;

        if (hitComponent != null)
        {
            hitComponent.Configure(
                data.damage,
                data.destroyOnHit,
                data.hitMaxTimes, // Updated to use hitMaxTimes instead of hitOnce
                data.hitCooldown
            );
        }

        if (rb != null)
        {
            rb.gravityScale = data.gravityScale;
        }

        if (debugMode)
            Debug.Log($"[Projectile:{gameObject.name}] Data updated at runtime");
    }
}
