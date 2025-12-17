using UnityEngine;
using System.Reflection; // for optional reflection checks

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class TriangleDetectionZone : MonoBehaviour
{
    // Optional interface you can implement on any player script for explicit hidden state
    public interface IHiddenState { bool IsHidden(); }

    [Header("Who gets busted")]
    public string playerTag = "Player";

    [Header("Timing")]
    [Tooltip("How long the player must remain inside before getting busted (seconds).")]
    public float requiredSecondsInside = 0.5f;

    [Header("Behavior")]
    [Tooltip("If true, once busted, must exit and re-enter to bust again.")]
    public bool bustOncePerEntry = true;

    [Tooltip("If true, the inside timer resets while TRANSFORMED; if false, it just pauses.")]
    public bool resetTimerWhenTransformed = true;

    [Tooltip("If true, the inside timer resets while HIDDEN; if false, it just pauses.")]
    public bool resetTimerWhenHidden = true;

    [Tooltip("Extra debug logs (hidden/transform checks, timer state).")]
    public bool verboseDebug = false;

    private GameObject playerRef;
    private PlayerTransform playerTransform;
    private float insideTimer = 0f;
    private bool hasBustedThisEntry = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerRef = other.gameObject;
        playerTransform = playerRef.GetComponent<PlayerTransform>();
        insideTimer = 0f;
        hasBustedThisEntry = false;

        if (verboseDebug) Debug.Log("[TriangleDetectionZone] ENTER: timer=0");
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (playerRef == null || !other.CompareTag(playerTag)) return;
        if (bustOncePerEntry && hasBustedThisEntry) return;

        // 1) Ignore while TRANSFORMED
        if (playerTransform != null && SafeIsTransformed(playerTransform))
        {
            if (resetTimerWhenTransformed) insideTimer = 0f;
            if (verboseDebug) Debug.Log("[TriangleDetectionZone] TRANSFORMED → timer " + (resetTimerWhenTransformed ? "reset" : "paused"));
            return; // pause timer
        }

        // 2) Ignore while HIDDEN
        if (IsPlayerHidden(playerRef.transform))
        {
            if (resetTimerWhenHidden) insideTimer = 0f;
            if (verboseDebug) Debug.Log("[TriangleDetectionZone] HIDDEN → timer " + (resetTimerWhenHidden ? "reset" : "paused"));
            return; // pause timer
        }

        // 3) Count only when fully detectable
        insideTimer += Time.deltaTime;

        if (insideTimer >= requiredSecondsInside)
        {
            // Safety re-check this frame
            if ((playerTransform != null && SafeIsTransformed(playerTransform)) || IsPlayerHidden(playerRef.transform))
                return;

            var move = playerRef.GetComponent<Movement>();
            if (move != null)
            {
                if (verboseDebug) Debug.Log("[TriangleDetectionZone] BUSTED → Movement.GetCaught()");
                move.GetCaught();
            }
            else
            {
                Debug.LogWarning("[TriangleDetectionZone] Player has no Movement component; cannot call GetCaught().");
            }

            hasBustedThisEntry = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerRef = null;
        playerTransform = null;
        insideTimer = 0f;
        hasBustedThisEntry = false;

        if (verboseDebug) Debug.Log("[TriangleDetectionZone] EXIT: timer cleared");
    }

    public void HardReset()
    {
        playerRef = null;
        playerTransform = null;
        insideTimer = 0f;
        hasBustedThisEntry = false;
    }

    public bool IsTargetInZone => playerRef != null && !hasBustedThisEntry;

    // --------------------------
    // Helpers
    // --------------------------

    private bool SafeIsTransformed(PlayerTransform pt)
    {
        try { return pt != null && pt.IsTransformed(); }
        catch { return false; }
    }

    // Hidden detection priority:
    // 1) Component implementing IHiddenState → IsHidden()
    // 2) Any component with bool IsHidden() method
    // 3) Any component with bool field/property: isHidden, isHiding, inHidingSpot, isInHidingSpot
    private bool IsPlayerHidden(Transform t)
    {
        if (t == null) return false;

        // 1) Interface (explicit & fastest)
        var iface = t.GetComponent<IHiddenState>();
        if (iface != null)
        {
            bool v = false;
            try { v = iface.IsHidden(); } catch { }
            if (verboseDebug) Debug.Log("[TriangleDetectionZone] Hidden via IHiddenState → " + v);
            if (v) return true;
        }

        // 2) Method IsHidden()
        var comps = t.GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            var m = c.GetType().GetMethod("IsHidden", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, System.Type.EmptyTypes, null);
            if (m != null && m.ReturnType == typeof(bool))
            {
                try
                {
                    bool v = (bool)m.Invoke(c, null);
                    if (verboseDebug) Debug.Log("[TriangleDetectionZone] Hidden via method IsHidden() on " + c.GetType().Name + " → " + v);
                    if (v) return true;
                }
                catch { }
            }
        }

        // 3) Common bool names (field/property)
        string[] names = { "isHidden", "isHiding", "inHidingSpot", "isInHidingSpot" };
        foreach (var c in comps)
        {
            if (c == null) continue;
            var type = c.GetType();

            foreach (var n in names)
            {
                var f = type.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null && f.FieldType == typeof(bool))
                {
                    try
                    {
                        bool v = (bool)f.GetValue(c);
                        if (verboseDebug) Debug.Log("[TriangleDetectionZone] Hidden via field " + type.Name + "." + n + " → " + v);
                        if (v) return true;
                    }
                    catch { }
                }
            }

            foreach (var n in names)
            {
                var p = type.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.PropertyType == typeof(bool) && p.CanRead)
                {
                    try
                    {
                        bool v = (bool)p.GetValue(c, null);
                        if (verboseDebug) Debug.Log("[TriangleDetectionZone] Hidden via property " + type.Name + "." + n + " → " + v);
                        if (v) return true;
                    }
                    catch { }
                }
            }
        }

        return false;
    }
}
