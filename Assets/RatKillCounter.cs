using TMPro;
using UnityEngine;

public class RatKillCounter : MonoBehaviour
{
    public static int RatsKilled { get; private set; }

    [SerializeField] private TMP_Text counterText;

    private static RatKillCounter instance;

    private void Awake()
    {
        // Ensure that there is only one instance of the RatKillCounter and initialize the kill count.
        instance = this;
        RatsKilled = 0;
        UpdateText();
    }

    public static void AddRatKill()
    {
        // Increment the rat kill count and update the UI text if the instance exists.
        RatsKilled++;

        if (instance != null)
        {
            instance.UpdateText();
        }
    }

    private void UpdateText()
    {
        // Update the counter text to display the current number of rats defeated.
        if (counterText != null)
        {
            counterText.text =
                "Rats Defeated: " + RatsKilled;
        }
    }
}