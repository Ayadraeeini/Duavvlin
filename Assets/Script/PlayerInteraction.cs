using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Input   InputReader)")]
    public InputReader input;

    private SprayBox currentSprayBox;

    void Awake()
    {
        if (!input) input = FindObjectOfType<InputReader>(); // fallback
    }

    void Update()
    {
        bool interactPressed =
            (input != null && input.InteractPressed()) ||
            Input.GetKeyDown(KeyCode.E); // fallback for keyboard

        if (interactPressed)
        {
            Debug.Log("[PlayerInteraction] Interact pressed");

            if (currentSprayBox != null)
            {
                Debug.Log("[PlayerInteraction] Found nearby SprayBox. Sending Paint()");
                currentSprayBox.Paint();
            }
            else
            {
                Debug.LogWarning("[PlayerInteraction] Interact pressed, but no SprayBox in range.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SprayBox"))
        {
            currentSprayBox = other.GetComponent<SprayBox>();
            Debug.Log($"[PlayerInteraction] Entered SprayBox area: {other.name}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("SprayBox") && currentSprayBox != null && currentSprayBox.gameObject == other.gameObject)
        {
            Debug.Log($"[PlayerInteraction] Exited SprayBox area: {other.name}");
            currentSprayBox = null;
        }
    }
}
