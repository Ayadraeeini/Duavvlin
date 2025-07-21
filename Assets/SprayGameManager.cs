// SprayGameManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SprayGameManager : MonoBehaviour
{
    public GameObject sprayPanel;
    public Image graffitiImage;
    public Transform slamTarget; // Where to place final image
    public GameObject graffitiPrefab; // Final graffiti that slams in
    public Texture2D sprayCursor;

    private bool gameRunning = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !gameRunning)
        {
            StartCoroutine(StartSprayMinigame());
        }
    }

    IEnumerator StartSprayMinigame()
    {
        gameRunning = true;

        sprayPanel.SetActive(true);
        Cursor.SetCursor(sprayCursor, Vector2.zero, CursorMode.Auto);

        // Wait for 7 seconds to simulate spraying
        yield return new WaitForSeconds(7f);

        sprayPanel.SetActive(false);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // Instantiate finished graffiti at slam location
        GameObject graffiti = Instantiate(graffitiPrefab, slamTarget.position, Quaternion.identity);
        gameRunning = false;
    }
}
