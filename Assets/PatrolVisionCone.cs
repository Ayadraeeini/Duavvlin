using UnityEngine;
using System.Reflection;   // for safe reflection of optional fields/properties

[RequireComponent(typeof(Collider2D))]
public class PatrolVisionCone : MonoBehaviour
{
    // --- Optional interface: implement on any player script for explicit hidden state.
    public interface IHiddenState
    {
        bool IsHidden();
    }

    [Header("Detection Settings")]
    [Tooltip("How long the player must remain inside the cone (seconds).")]
    public float requiredSecondsInCone = 0.6f;
    public string playerTag = "Player";

    [Header("Transformed Behavior")]
    [Tooltip("If true: timer resets while transformed. If false: timer pauses.")]
    public bool resetTimerWhenTransformed = true;

    [Header("Hidden Behavior")]
    [Tooltip("If true: timer resets while HIDDEN. If false: timer pauses.")]
    public bool resetTimerWhenHidden = true;

    [Tooltip("Enable extra debug logs to verify hidden/transform checks.")]
    public bool verboseDebug = false;

    private float playerTimer = 0f;
    private bool playerInside = false;
    private Transform playerInsideTrigger;
    private PlayerTransform playerTransform;   // cached for quick checks
    private bool hasTriggered = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Update()
    {
        if (!playerInside || playerInsideTrigger == null || hasTriggered)
            return;

        // 1) Ignore while TRANSFORMED (can form)
        if (playerTransform != null && SafeIsTransformed(playerTransform))
        {
            if (resetTimerWhenTransformed) playerTimer = 0f; // fully protected
            if (verboseDebug) Debug.Log("[PatrolVisionCone] Player transformed → timer " + (resetTimerWhenTransformed ? "reset" : "paused"));
            return; // pause counting
        }

        // 2) Ignore while HIDDEN (in hiding spot)
        if (IsPlayerHidden(playerInsideTrigger))
        {
            if (resetTimerWhenHidden) playerTimer = 0f; // fully protected while hidden
            if (verboseDebug) Debug.Log("[PatrolVisionCone] Player hidden → timer " + (resetTimerWhenHidden ? "reset" : "paused"));
            return; // pause counting
        }

        // 3) Count time only when fully detectable
        playerTimer += Time.deltaTime;

        if (playerTimer >= requiredSecondsInCone)
        {
            hasTriggered = true;
            TriggerBusted();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = true;
        playerInsideTrigger = other.transform;
        playerTransform = playerInsideTrigger.GetComponent<PlayerTransform>(); // may be null
        playerTimer = 0f;
        hasTriggered = false;

        if (verboseDebug) Debug.Log("[PatrolVisionCone] Player ENTER cone.");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform != playerInsideTrigger)
            return;

        playerInside = false;
        playerTimer = 0f;
        hasTriggered = false;
        playerInsideTrigger = null;
        playerTransform = null;

        if (verboseDebug) Debug.Log("[PatrolVisionCone] Player EXIT cone. Timer cleared.");
    }

    private void TriggerBusted()
    {
        if (playerInsideTrigger == null)
            return;

        // Final safety: skip if transformed or hidden this frame
        if ((playerTransform != null && SafeIsTransformed(playerTransform)) || IsPlayerHidden(playerInsideTrigger))
            return;

        var movement = playerInsideTrigger.GetComponent<Movement>();
        if (movement != null)
        {
            if (verboseDebug) Debug.Log("[PatrolVisionCone] BUSTED: calling Movement.GetCaught()");
            movement.GetCaught(); // your Movement.cs method
        }
        else
        {
            Debug.LogWarning("[PatrolVisionCone] No Movement component found on player.");
        }
    }

    // Optional external reset
    public void HardResetUI()
    {
        playerTimer = 0f;
        playerInside = false;
        playerInsideTrigger = null;
        playerTransform = null;
        hasTriggered = false;
    }

    // Optional helper for AI, etc.
    public bool IsTargetInSight => playerInside && !hasTriggered;

    // --------------------------
    // Helpers
    // --------------------------

    // Some users expose IsTransformed as method; keep it safe.
    private bool SafeIsTransformed(PlayerTransform pt)
    {
        try { return pt != null && pt.IsTransformed(); }
        catch { return false; }
    }

    // Unified hidden detection:
    // 1) Interface IHiddenState → IsHidden()
    // 2) Any component with method bool IsHidden()
    // 3) Any component with a bool field/property named: isHidden, isHiding, inHidingSpot, isInHidingSpot
    private bool IsPlayerHidden(Transform t)
    {
        if (t == null) return false;

        // 1) Interface first (fast & explicit)
        var hiddenInterface = t.GetComponent<IHiddenState>();
        if (hiddenInterface != null)
        {
            bool val = false;
            try { val = hiddenInterface.IsHidden(); } catch { }
            if (verboseDebug) Debug.Log("[PatrolVisionCone] Hidden via IHiddenState → " + val);
            return val;
        }

        // 2) Search components for method bool IsHidden()
        var comps = t.GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            var m = c.GetType().GetMethod("IsHidden", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
            if (m != null && m.ReturnType == typeof(bool))
            {
                try
                {
                    bool val = (bool)m.Invoke(c, null);
                    if (verboseDebug) Debug.Log("[PatrolVisionCone] Hidden via method IsHidden() on " + c.GetType().Name + " → " + val);
                    if (val) return true;
                }
                catch { }
            }
        }

        // 3) Look for common bool fields/properties
        string[] names = { "isHidden", "isHiding", "inHidingSpot", "isInHidingSpot" };
        foreach (var c in comps)
        {
            if (c == null) continue;
            var type = c.GetType();

            // field
            foreach (var n in names)
            {
                var f = type.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null && f.FieldType == typeof(bool))
                {
                    try
                    {
                        bool val = (bool)f.GetValue(c);
                        if (verboseDebug) Debug.Log("[PatrolVisionCone] Hidden via field " + type.Name + "." + n + " → " + val);
                        if (val) return true;
                    }
                    catch { }
                }
            }

            // property
            foreach (var n in names)
            {
                var p = type.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.PropertyType == typeof(bool) && p.CanRead)
                {
                    try
                    {
                        bool val = (bool)p.GetValue(c, null);
                        if (verboseDebug) Debug.Log("[PatrolVisionCone] Hidden via property " + type.Name + "." + n + " → " + val);
                        if (val) return true;
                    }
                    catch { }
                }
            }
        }

        return false;
    }
}
