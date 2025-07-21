using UnityEngine;

public class Spray : MonoBehaviour
{
    public GameObject graffitiUI;
    private bool inRange;

    void Start()
    {
        graffitiUI.SetActive(false); // Hide panel at start
    }

    void Update()
    {
        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            graffitiUI.SetActive(true); // Show graffiti UI
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            inRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            graffitiUI.SetActive(false); // Hide UI when leaving
        }
    }
}
