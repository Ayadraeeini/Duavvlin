using UnityEngine;
using TMPro;
using System.Collections;

public class InstructionTrigger : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public float fadeDuration = 0.5f;

    private Coroutine currentFade;

    void Start()
    {
        if (instructionText != null)
        {
            Color c = instructionText.color;
            c.a = 0f;
            instructionText.color = c;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && instructionText != null)
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeTextAlpha(1f));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && instructionText != null)
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeTextAlpha(0f));
        }
    }

    IEnumerator FadeTextAlpha(float targetAlpha)
    {
        float startAlpha = instructionText.color.a;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            Color newColor = instructionText.color;
            newColor.a = alpha;
            instructionText.color = newColor;
            yield return null;
        }

        Color finalColor = instructionText.color;
        finalColor.a = targetAlpha;
        instructionText.color = finalColor;
    }
}
