using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Health catHealth;
    [SerializeField] private TMP_Text healthText;


    private void Update()
    {
        // Update the health UI text to reflect the current health of the cat.
        if (catHealth == null || healthText == null)
        {
            return;
        }

        healthText.text =
            "HP: " +
            Mathf.CeilToInt(catHealth.CurrentHealth) +
            " / " +
            Mathf.CeilToInt(catHealth.MaxHealth);
    }
}