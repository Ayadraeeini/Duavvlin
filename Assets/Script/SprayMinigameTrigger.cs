using UnityEngine;

public class SprayMinigameTrigger : MonoBehaviour
{
    public GameObject sprayPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            sprayPanel.SetActive(true); // Show the coloring page!
        }
    }
}
