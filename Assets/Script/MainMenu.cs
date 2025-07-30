using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called when "Start" button is pressed
    public void PlayGame()
    {
        // Load scene named "TutorialLevel"
        SceneManager.LoadScene("TutorialLevel");
    }

    // Called when "Options" button is pressed
    public void OpenOptions()
    {
        // Add your options menu UI logic here
        Debug.Log("Options menu opened");
    }

    // Called when "Credits" button is pressed
    public void OpenCredits()
    {
        // Add your credits UI logic here
        Debug.Log("Credits shown");
    }

    // Called when "Exit" button is pressed
    public void QuitGame()
    {
        Debug.Log("Quit game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
