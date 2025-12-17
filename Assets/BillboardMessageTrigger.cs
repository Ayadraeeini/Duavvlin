using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BillboardMessageTrigger : MonoBehaviour
{
    [Header("Who can trigger")]
    public string playerTag = "Player";

    [Header("UI References (from the HUD)")]
    public CanvasGroup bannerGroup;     // CanvasGroup on BillboardMessage panel
    public TMP_Text messageText;        // TextMeshProUGUI on BillboardMessageText

    [Header("Message")]
    [TextArea(2, 5)]
    public string[] lines = new string[]
    {
        "Tag the billboard and let the city know you.",
        "Put your mark up. Make the skyline remember.",
        "Billboard’s bare—dress it in your legend.",
        "Own the night. Paint your name across it.",
        "No whispers—spray it loud so the streets never forget."
    };

    [Header("Timing (seconds)")]
    public float fadeIn = 0.25f;
    public float hold = 2.75f;
    public float fadeOut = 0.6f;

    [Header("Behavior")]
    public bool oneShot = true;   // trigger only once per run

    bool _fired;

    void Awake()
    {
        if (bannerGroup) bannerGroup.alpha = 0f; // start hidden
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_fired && oneShot) return;
        if (!other.CompareTag(playerTag)) return;

        if (!bannerGroup || !messageText)
        {
            Debug.LogError("[BillboardMessageTrigger] Assign bannerGroup (CanvasGroup) and messageText (TMP_Text) in Inspector.");
            return;
        }

        // Pick a line (default to first if array empty)
        string line = (lines != null && lines.Length > 0) ? lines[Random.Range(0, lines.Length)]
                                                          : "Tag the billboard and let the city know you.";

        messageText.text = line;

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());

        _fired = true;
    }

    IEnumerator ShowRoutine()
    {
        yield return FadeTo(1f, fadeIn);       // fade in
        yield return new WaitForSeconds(hold); // hold
        yield return FadeTo(0f, fadeOut);      // fade out
    }

    IEnumerator FadeTo(float target, float duration)
    {
        float start = bannerGroup.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // UI not affected by timescale changes
            bannerGroup.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        bannerGroup.alpha = target;
    }
}
