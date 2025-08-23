using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class CopInstantBuster : MonoBehaviour
{
    [Header("Who & UI")]
    public Transform player;                 // auto-find by tag if null
    public EyeDetectionUI eyeUI;             // auto-find under Player if null

    [Header("Behavior")]
    public float eyeDuration = 2f;           // how long to show the eye before bust
    public bool cancelIfStealthed = true;    // abort if player hides/transforms mid-countdown

    [Header("Debug")]
    public bool logEvents = false;

    // runtime
    Movement pm;
    Coroutine countdown;
    bool counting;

    void Awake()
    {
        // Setup LineRenderer so it's visible in 2D (URP/HDRP safe)
        var lr = GetComponent<LineRenderer>();
        if (lr)
        {
            if (lr.sharedMaterial == null)
                lr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            lr.positionCount = 0;         // we just want a visible marker (optional)
            lr.startWidth = 0.03f;
            lr.endWidth = 0.03f;
            lr.startColor = Color.yellow;
            lr.endColor = Color.yellow;
            var rend = lr.GetComponent<Renderer>();
            if (rend != null) { rend.sortingLayerName = "Characters"; rend.sortingOrder = 200; }
        }

        var poly = GetComponent<PolygonCollider2D>();
        poly.isTrigger = true;
    }

    void Start()
    {
        if (player == null)
        {
            var found = GameObject.FindWithTag("Player");
            if (found) player = found.transform;
        }
        if (player != null)
        {
            pm = player.GetComponent<Movement>();
            if (eyeUI == null) eyeUI = player.GetComponentInChildren<EyeDetectionUI>(true);
        }

        if (player == null) Debug.LogWarning($"[{name}] No Player (tag 'Player') found.");
        if (eyeUI == null) Debug.LogWarning($"[{name}] No EyeDetectionUI found under Player.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        TryStartCountdown();
    }

    // Sticky by design: exiting the cone does NOT cancel.
    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        // no cancel; if you want cancel on exit, call: CancelCountdown(resetEye:true);
    }

    void OnDisable()
    {
        CancelCountdown(resetEye: true);
    }

    bool IsPlayer(Collider2D c)
    {
        if (player == null) return c.CompareTag("Player");
        return (c.attachedRigidbody ? c.attachedRigidbody.transform : c.transform) == player;
    }

    void TryStartCountdown()
    {
        if (counting) return;
        if (pm != null && pm.IsStealthed() && cancelIfStealthed) return;

        if (logEvents) Debug.Log($"[{name}] DETECT: start 2s eye → bust.");
        countdown = StartCoroutine(CountdownThenBust());
        counting = true;
    }

    IEnumerator CountdownThenBust()
    {
        float t = 0f;

        // Make sure the eye becomes visible immediately
        if (eyeUI != null) eyeUI.ResetDetection();

        // keep the eye "on" during the whole countdown
        while (t < eyeDuration)
        {
            if (eyeUI != null) eyeUI.StartDetection(999f); // slam the meter so it's clearly on

            if (cancelIfStealthed && pm != null && pm.IsStealthed())
            {
                if (logEvents) Debug.Log($"[{name}] DETECT: canceled (stealthed).");
                if (eyeUI != null) eyeUI.ResetDetection();
                counting = false;
                countdown = null;
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Bust
        if (eyeUI != null) eyeUI.ResetDetection();
        if (pm != null) pm.GetCaught();
        if (logEvents) Debug.Log($"[{name}] DETECT: BUSTED.");

        counting = false;
        countdown = null;
    }

    public void HardResetDetection()
    {
        CancelCountdown(resetEye: true);
    }

    void CancelCountdown(bool resetEye)
    {
        if (countdown != null) StopCoroutine(countdown);
        if (resetEye && eyeUI != null) eyeUI.ResetDetection();
        counting = false;
        countdown = null;
    }
}
