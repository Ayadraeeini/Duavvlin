using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(1); // 2
                                   // means the scene at index 1 in Build Settings
    }

    public void Credits()
    {
        SceneManager.LoadScene(2); // 2
                                   // means the scene at index 1 in Build Settings
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }

    public void Menu()
    {
        SceneManager.LoadScene(0);
    }
}
