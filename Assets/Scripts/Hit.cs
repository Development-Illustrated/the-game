using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

// Interface for objects that can hit others
public interface IHitter
{
    void ApplyDamage(GameObject target);
}

public class Hit : MonoBehaviour, IHitter
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;

    [Header("Hit Behavior")]
    [SerializeField] private bool destroyOnHit = false;
    [SerializeField] private int hitMaxTimes = 1;
    [SerializeField] private float hitCooldown = 0.5f;
    [SerializeField] private bool debugMode = false;

    [Header("Events")]
    public UnityEvent<GameObject> OnHitObject;
    public UnityEvent<GameObject> OnBeforeDestroy;

    private Dictionary<GameObject, int> hitCounts = new Dictionary<GameObject, int>(); // Track hit counts
    private Dictionary<GameObject, float> hitCooldowns = new Dictionary<GameObject, float>();

    public void Configure(float damage, bool destroyOnHit, int hitMaxTimes, float hitCooldown)
    {
        this.damage = damage;
        this.destroyOnHit = destroyOnHit;
        this.hitMaxTimes = hitMaxTimes;
        this.hitCooldown = hitCooldown;

        if (debugMode)
            Debug.Log($"[Hit:{gameObject.name}] Configured with damage={damage}, destroyOnHit={destroyOnHit}, hitMaxTimes={hitMaxTimes}, cooldown={hitCooldown}");
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetDestroyOnHit(bool destroyOnHit)
    {
        this.destroyOnHit = destroyOnHit;
    }

    public void SetHitMaxTimes(int hitMaxTimes)
    {
        this.hitMaxTimes = hitMaxTimes;
    }

    public void SetHitCooldown(float hitCooldown)
    {
        this.hitCooldown = hitCooldown;
    }

    public void SetDebugMode(bool debugMode)
    {
        this.debugMode = debugMode;
    }

    private void Start()
    {
        if (debugMode)
            Debug.Log($"[Hit:{gameObject.name}] Initialized with damage={damage}, destroyOnHit={destroyOnHit}, hitMaxTimes={hitMaxTimes}");
    }

    void Update()
    {
        // Manage cooldowns
        List<GameObject> objectsToRemove = new List<GameObject>();

        // Create a copy of the keys to avoid collection modification during enumeration
        foreach (var obj in hitCooldowns.Keys.ToList())
        {
            if (obj == null)
            {
                objectsToRemove.Add(obj);
                continue;
            }

            hitCooldowns[obj] -= Time.deltaTime;
            if (hitCooldowns[obj] <= 0)
            {
                objectsToRemove.Add(obj);
            }
        }

        foreach (var obj in objectsToRemove)
        {
            hitCooldowns.Remove(obj);
            if (debugMode && obj != null)
                Debug.Log($"[Hit:{gameObject.name}] Cooldown expired for {obj.name}, can hit again if under max hits");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (debugMode)
            Debug.Log($"[Hit:{gameObject.name}] Trigger enter with {other.gameObject.name}");
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (debugMode)
            Debug.Log($"[Hit:{gameObject.name}] Collision enter with {collision.gameObject.name}");
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject target)
    {
        // Skip if max hit count reached
        if (hitCounts.ContainsKey(target) && hitCounts[target] >= hitMaxTimes && hitMaxTimes > 0)
        {
            if (debugMode)
                Debug.Log($"[Hit:{gameObject.name}] Skipping {target.name} - already hit {hitCounts[target]} times (max: {hitMaxTimes})");
            return;
        }

        // Skip if on cooldown
        if (hitCooldowns.ContainsKey(target))
        {
            if (debugMode)
                Debug.Log($"[Hit:{gameObject.name}] Skipping {target.name} - on cooldown for {hitCooldowns[target]} more seconds");
            return;
        }

        ApplyDamage(target);
    }

    public void ApplyDamage(GameObject target)
    {
        IHurtable hurtable = target.GetComponent<IHurtable>();

        if (hurtable != null)
        {
            if (debugMode)
                Debug.Log($"[Hit:{gameObject.name}] Applying {damage} damage to {target.name}");

            hurtable.TakeDamage(damage, gameObject);

            // Update hit counter
            if (!hitCounts.ContainsKey(target))
                hitCounts[target] = 0;
            hitCounts[target]++;

            if (debugMode)
                Debug.Log($"[Hit:{gameObject.name}] Hit count for {target.name}: {hitCounts[target]}/{hitMaxTimes}");

            // Add cooldown
            hitCooldowns[target] = hitCooldown;

            // Fire event
            OnHitObject?.Invoke(target);

            if (destroyOnHit)
            {
                if (debugMode)
                    Debug.Log($"[Hit:{gameObject.name}] Destroying self after hitting {target.name}");

                // Fire the pre-destroy event
                OnBeforeDestroy?.Invoke(target);

                Destroy(gameObject);
            }
        }
        else if (debugMode)
        {
            Debug.Log($"[Hit:{gameObject.name}] Target {target.name} doesn't have IHurtable component, can't apply damage");
        }
    }
}
