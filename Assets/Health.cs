using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private AudioClip hurtSound;

    private AudioSource audioSource;

    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public UnityEvent onDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    // Method to apply damage to the entity, reducing its health and triggering death if health reaches zero.
    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHealth <= 0f)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        Debug.Log($"{gameObject.name} health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    // Method to handle the death of the entity, invoking any death events and destroying the game object.
    private void Die()
    {
        onDeath?.Invoke();

        Debug.Log($"{gameObject.name} died.");

        if (gameObject.CompareTag("Player"))
        {
            Debug.Log("Game Over!");
        }

        if (gameObject.CompareTag("Rat"))
        {
            RatKillCounter.AddRatKill();
        }

        Destroy(gameObject);
    }
}