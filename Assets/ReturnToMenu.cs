using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    [Header("Main Menu Scene Name")]
    public string mainMenuSceneName = "MainMenu"; // Change to your actual menu scene name

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object entering is the player
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
