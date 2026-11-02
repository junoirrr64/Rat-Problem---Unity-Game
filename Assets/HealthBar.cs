using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Health targetHealth;
    [SerializeField] private Slider healthSlider;

    // Initialize the health slider's min and max values based on the target health.
    private void Start()
    {
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
    }

    // Update the health slider's value to reflect the current health percentage of the target.
    private void Update()
    {
        if (targetHealth == null)
            return;

        healthSlider.value =
            targetHealth.CurrentHealth /
            targetHealth.MaxHealth;
    }
}