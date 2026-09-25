using UnityEngine;
using UnityEngine.Events;

// Interface for objects that can be hurt
public interface IHurtable
{
    void TakeDamage(float amount, GameObject damageSource);
    void SetInvulnerable(bool invulnerable);
    bool IsInvulnerable();
}

public class Hurt : MonoBehaviour, IHurtable
{
    [Header("Hurt Settings")]
    [SerializeField] private bool isInvulnerable = false;
    [SerializeField] private bool debugMode = false;

    [Header("Events")]
    public UnityEvent<float, GameObject> OnDamaged;

    private void Start()
    {
        if (debugMode)
            Debug.Log($"[Hurt:{gameObject.name}] Initialized with invulnerable={isInvulnerable}");
    }

    public void TakeDamage(float amount, GameObject damageSource)
    {
        if (isInvulnerable)
        {
            if (debugMode)
                Debug.Log($"[Hurt:{gameObject.name}] Ignored {amount} damage from {(damageSource ? damageSource.name : "null")} (invulnerable)");
            return;
        }

        if (debugMode)
            Debug.Log($"[Hurt:{gameObject.name}] Received {amount} damage from {(damageSource ? damageSource.name : "null")}");

        // Simply pass the damage to listeners
        OnDamaged?.Invoke(amount, damageSource);
    }

    public void SetInvulnerable(bool invulnerable)
    {
        if (isInvulnerable != invulnerable)
        {
            isInvulnerable = invulnerable;

            if (debugMode)
                Debug.Log($"[Hurt:{gameObject.name}] Invulnerability set to {isInvulnerable}");
        }
    }

    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }
}
