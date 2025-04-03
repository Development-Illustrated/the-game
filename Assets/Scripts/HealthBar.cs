using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HealthBar : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [SerializeField] private GameObject healthBar;
    [SerializeField] private bool isDead = false;

    [SerializeField] private Gradient colorGradient;
    [SerializeField] private GameObject healthBarFill;
    private Slider healthBarSlider;

    [Header("Events")]
    public UnityEvent<float, GameObject> OnDamaged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBarSlider = healthBar.GetComponent<Slider>();

        healthBarSlider.minValue = 0;
        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = maxHealth; // Set an initial value for the slider

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
        OnDamaged.AddListener(TakeDamage);
    }

    // Update is called once per frame
    void Update()
    {
        if (colorGradient == null)
        {
            Debug.LogError("colorGradient is not assigned.");
        }
        // Get the color based on the current health (from 0 to 100)
        Color color = colorGradient.Evaluate(currentHealth / 100f);

        // Set the object's material color
        healthBarFill.GetComponent<Image>().color = color;
    }

    void TakeDamage(float amount, GameObject damageSource)
    {
        if (isDead) return;

        healthBarSlider.value -= (int)amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
    }
}
