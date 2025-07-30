using UnityEngine;

public class PlaySoundWhenClose : MonoBehaviour
{
    public Transform player;          // Reference to the player object
    private AudioSource audioSource;  // The AudioSource to play the sound
    public float minX = 18f;         // Minimum x-coordinate for the sound to trigger
    public float maxX = 20.5f;       // Maximum x-coordinate for the sound to trigger

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource attached to the "Closed" sign
    }

    void Update()
    {
        // Get the player's x position
        float playerX = player.position.x;

        // Debug the player's x position
        Debug.Log("Player X Position: " + playerX);

        // Check if the player is within the x-range (18 <= x <= 20.5)
        if (playerX >= minX && playerX <= maxX)
        {
            // Check if the sound is not already playing
            if (!audioSource.isPlaying)
            {
                Debug.Log("Player is in range (x) and sound will play.");
                audioSource.Play();  // Play the sound
            }
        }
        else
        {
            // Stop the sound if the player is out of range
            if (audioSource.isPlaying)
            {
                Debug.Log("Player is out of range (x). Stopping sound.");
                audioSource.Stop(); // Stop the sound if the player moves away
            }
        }
    }
}
