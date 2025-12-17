using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoIntroPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "MainMenu";  // Your main menu scene name

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
