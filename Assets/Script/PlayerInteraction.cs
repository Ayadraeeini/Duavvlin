using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private SprayBox currentSprayBox;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[PlayerInteraction] Pressed E");

            if (currentSprayBox != null)
            {
                Debug.Log("[PlayerInteraction] Found nearby SprayBox. Sending Paint()");
                currentSprayBox.Paint();
            }
            else
            {
                Debug.LogWarning("[PlayerInteraction] Pressed E, but no SprayBox in range.");
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
        if (other.CompareTag("SprayBox"))
        {
            if (currentSprayBox != null && currentSprayBox.gameObject == other.gameObject)
            {
                Debug.Log($"[PlayerInteraction] Exited SprayBox area: {other.name}");
                currentSprayBox = null;
            }
        }
    }
}
