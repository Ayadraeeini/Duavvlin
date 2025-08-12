using UnityEngine;
using UnityEngine.SceneManagement; // Needed for loading scenes

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private string sceneName = "Level2"; // Name of your level

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Only works if Player has tag "Player"
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
