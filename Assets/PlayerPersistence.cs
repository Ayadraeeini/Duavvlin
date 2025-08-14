using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    private static PlayerPersistence instance;
    private Movement movement; // your movement script

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // prevent duplicates if scene also has a Player
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        movement = GetComponent<Movement>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find a spawn point in the new scene
        SpawnPoint sp = FindObjectOfType<SpawnPoint>();
        if (sp != null)
        {
            // Move player and reset state so nothing carries over weirdly
            if (movement != null)
                movement.ResetForNewScene(sp.transform);
            else
                transform.position = sp.transform.position;
        }
    }
}
