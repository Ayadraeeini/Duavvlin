using UnityEngine;

public class ClosedSignFlicker : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;  // This will help us change the sign's look
    public float minFlickerTime = 0.1f;    // How fast the flicker happens (minimum time)
    public float maxFlickerTime = 0.5f;    // How fast the flicker happens (maximum time)
    public float minAlpha = 0.2f;          // How see-through the sign can get (minimum)
    public float maxAlpha = 1.0f;          // How bright the sign can be (maximum)

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the picture on the object
        StartFlickering();  // Start the flicker effect
    }

    private void StartFlickering()
    {
        // Start the flicker and make it happen over and over at random times
        InvokeRepeating("Flicker", 0f, Random.Range(minFlickerTime, maxFlickerTime));
    }

    private void Flicker()
    {
        // Make the sign randomly bright or faint
        float alpha = Random.Range(minAlpha, maxAlpha);
        Color color = spriteRenderer.color;  // Get the current color of the sign
        color.a = alpha;  // Change how see-through it is
        spriteRenderer.color = color;  // Apply the new color
    }

    private void OnDisable()
    {
        CancelInvoke("Flicker");  // Stop the flicker when the sign is turned off
    }
}
