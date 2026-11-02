using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text messageText;

    private void Awake()
    {
        // Ensure the game over panel is hidden and the game time scale is normal at the start.
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Check for the 'N' key press to restart the game if the game over panel is active.
        if (gameOverPanel != null &&
            gameOverPanel.activeSelf &&
            Keyboard.current != null &&
            Keyboard.current.nKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    public void ShowGameOver()
    {
        // Display the game over panel and set the message text to indicate that the player has died.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (messageText != null)
        {
            messageText.text = "YOU DIED\nPRESS N TO RESTART";
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // Restart the current scene and reset the game time scale to normal.
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}