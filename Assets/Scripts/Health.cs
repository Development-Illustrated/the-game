using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isDead = false;
    [SerializeField] private GameObject player;

    [SerializeField] private Renderer playerRenderer;

    [SerializeField] private Gradient colorGradient;

    [Header("Events")]
    public UnityEvent<float, GameObject> OnDamaged;
    public UnityEvent<float, GameObject> SetHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Define the color gradient: Green -> Amber -> Red
        colorGradient = new Gradient();
        colorGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(Color.green, 1f),  // Green at 0
            new GradientColorKey(new Color(1f, 0.75f, 0f), 0.42f), // Amber
            new GradientColorKey(new Color(1f, 0.75f, 0f), 0.58f),
            new GradientColorKey(Color.red, 0f)   // Red at 100
        };
        colorGradient.alphaKeys = new GradientAlphaKey[]
        {
            new GradientAlphaKey(1f, 0f),  // Full opacity at 0
            new GradientAlphaKey(1f, 1f)   // Full opacity at 100
        };

        currentHealth = maxHealth;
        SetHealth?.Invoke(maxHealth, player);
        OnDamaged.AddListener(TakeDamage);
    }

    // Update is called once per frame
    void Update()
    {
        // Get the color based on the current health (from 0 to 100)
        Color color = colorGradient.Evaluate(currentHealth / 100f);

        // Set the object's material color
        playerRenderer.material.color = color;
    }

    public void TakeDamage(float amount, GameObject damageSource)
    {
        Debug.Log($"[Health:{gameObject.name}] Received {amount} damage from {(damageSource ? damageSource.name : "null")}");
        if (isDead) return;

        currentHealth -= (int)amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        // Handle death logic here (e.g., play animation, disable components, etc.)
        // For example, destroy the game object
        Destroy(gameObject);
        // Respawn();
    }
}
