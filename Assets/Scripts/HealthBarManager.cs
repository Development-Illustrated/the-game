using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // For UnityEvent handling, if needed

public class HealthBarManager : MonoBehaviour
{
    public GameObject sliderPrefab; // The prefab for the slider UI element
    public Transform sliderContainer; // The container (e.g., Panel) that holds sliders
    public int playerCount = 0; // Keep track of the number of players (this should sync with your multiplayer system)
    private GameObject player; // Reference to the player object

    [Header("Events")]
    UnityEvent<float, GameObject> SetHealth;

    void Start()
    {
        SetHealth.AddListener(AddSliderForPlayer);
    }

    // Call this function when a player joins the game
    public void AddSliderForPlayer(float maxHealth, GameObject player)
    {
        // Instantiate a new slider from the prefab
        GameObject newSlider = Instantiate(sliderPrefab, sliderContainer);
        
        // Optionally, set slider properties such as a label or initial value
        Slider sliderComponent = newSlider.GetComponent<Slider>();
        
        // Customize the slider as needed (e.g., set value range, onValueChanged event, etc.)
        sliderComponent.minValue = 0;
        sliderComponent.maxValue = maxHealth;
        sliderComponent.value = maxHealth; // Set an initial value for the slider
    }
}