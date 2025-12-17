using UnityEngine;

public class CreditsTrigger : MonoBehaviour
{
    public CreditsRoll credits;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            credits.StartCredits();
        }
    }
}
