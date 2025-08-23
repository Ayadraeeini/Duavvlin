using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CreditsRoll : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI creditsText;         // Drag in your big TMP text object
    public CanvasGroup creditsGroup;            // Drag in the canvas group to fade in

    [Header("Timing")]
    public float fadeInDuration = 2f;           // Time to fade in the canvas
    public float delayBetweenSections = 1.5f;   // Delay before showing next credit block
    public float finalDelay = 3f;               // Wait at the end before ending

    [Header("Credits Text")]
    [TextArea(5, 20)]
    public List<string> creditSections = new List<string>();

    [Header("Player Control")]
    public Movement movementScript;             // Drag your player’s Movement.cs here
    public float autoWalkSpeed = 2f;            // Speed the player walks automatically

    private bool hasStarted = false;

    public void StartCredits()
    {
        if (hasStarted) return;
        hasStarted = true;

        // Lock player control + start walking
        if (movementScript != null)
        {
            movementScript.DisablePlayerInput();
            movementScript.AutoWalk(autoWalkSpeed);
        }

        StartCoroutine(RunCredits());
    }

    IEnumerator RunCredits()
    {
        // Fade in the UI
        creditsGroup.alpha = 0f;
        creditsGroup.gameObject.SetActive(true);

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            creditsGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }

        // Build the full credit block with consistent spacing
        creditsText.text = "<line-height=80%>";  // Start tag to shrink spacing globally

        for (int i = 0; i < creditSections.Count; i++)
        {
            creditsText.text += creditSections[i];

            if (i < creditSections.Count - 1)
                creditsText.text += "\n"; // Single consistent line break between all
        }

        // Reveal one section at a time with timing
        for (int i = 0; i < creditSections.Count; i++)
        {
            yield return new WaitForSeconds(delayBetweenSections);
        }

        yield return new WaitForSeconds(finalDelay);

        Debug.Log("Credits finished.");

        // Optional: Fade out, return to menu, etc.
    }
}
