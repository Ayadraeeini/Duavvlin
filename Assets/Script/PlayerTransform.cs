using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;       // for Legacy Text
using TMPro;                // for TextMeshProUGUI

public class PlayerTransform : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite transformedSprite;

    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.T;

    [Header("Timing")]
    [Tooltip("How long the scale-in/out animation takes.")]
    public float transitionTime = 0.5f;

    [Tooltip("Auto revert after this many seconds.")]
    public float maxTransformDuration = 5f; // exactly 5s

    [Header("Duration Control")]
    [Tooltip("If true, player cannot manually revert early; transformation lasts the full duration.")]
    public bool forceFullDuration = true;

    [Header("Scale")]
    public Vector3 canScale = new Vector3(0.5f, 0.5f, 1f);

    [Header("Movement While Transformed")]
    [Tooltip("Keep the player able to move while transformed.")]
    public bool allowMovementWhileTransformed = true;
    [Tooltip("Optional speed multiplier while transformed (1 = same speed).")]
    public float transformedSpeedMultiplier = 1f;

    [Header("Countdown UI (optional)")]
    [Tooltip("Assign a TextMeshProUGUI for UI canvas countdown (preferred).")]
    public TextMeshProUGUI countdownTMP;
    [Tooltip("Or assign a Legacy Text if not using TMP.")]
    public Text countdownText;
    [Tooltip("Fade the countdown in/out automatically.")]
    public bool fadeCountdown = true;
    [Tooltip("How fast the countdown fades in/out.")]
    public float countdownFadeTime = 0.15f;

    // Components & state
    private SpriteRenderer sr;
    private Animator animator;
    private Movement movement; // optional
    private Vector3 originalScale;
    private bool isTransformed = false;
    private bool isTransitioning = false;

    // Coroutines
    private Coroutine activeTransition;
    private Coroutine autoRevertRoutine;
    private Coroutine countdownFadeRoutine;

    // Optional event
    public event Action<bool> OnTransformedChanged;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        movement = GetComponent<Movement>(); // ok if null

        originalScale = transform.localScale;

        if (normalSprite != null) sr.sprite = normalSprite;
        if (animator == null) Debug.LogWarning("Animator is missing from Player!");

        // Ensure countdown hidden at start
        SetCountdownVisible(false, true);
        SetCountdownText(""); // clear
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleTransform();
        }
    }

    public void ToggleTransform()
    {
        if (isTransitioning) return;

        // Enforce full 5s duration when already transformed
        if (isTransformed && forceFullDuration) return;

        if (!isTransformed) StartTransformIn();
        else StartTransformOut();
    }

    void StartTransformIn()
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        if (autoRevertRoutine != null) { StopCoroutine(autoRevertRoutine); autoRevertRoutine = null; }

        activeTransition = StartCoroutine(Co_TransformIn());
    }

    void StartTransformOut()
    {
        if (activeTransition != null) StopCoroutine(activeTransition);
        if (autoRevertRoutine != null) { StopCoroutine(autoRevertRoutine); autoRevertRoutine = null; }

        activeTransition = StartCoroutine(Co_TransformOut());
    }

    IEnumerator Co_TransformIn()
    {
        isTransitioning = true;

        if (animator != null) animator.SetTrigger("ToCan");

        // scale down
        yield return StartCoroutine(ScaleOverTime(transform, transform.localScale, canScale, transitionTime));

        // swap sprite (optional)
        // if (transformedSprite != null) sr.sprite = transformedSprite;

        isTransformed = true;
        isTransitioning = false;
        OnTransformedChanged?.Invoke(true);

        // movement stays enabled; optionally adjust speed
        if (allowMovementWhileTransformed && movement != null)
        {
            movement.SetSpeedMultiplier(transformedSpeedMultiplier); // Movement has this now
        }

        // show + run countdown and auto-revert
        if (maxTransformDuration > 0f)
        {
            autoRevertRoutine = StartCoroutine(Co_AutoRevertWithCountdown(maxTransformDuration));
        }
    }

    IEnumerator Co_TransformOut()
    {
        isTransitioning = true;

        if (animator != null) animator.SetTrigger("ToHuman");

        // hide countdown immediately
        SetCountdownVisible(false);

        // scale back up
        yield return StartCoroutine(ScaleOverTime(transform, transform.localScale, originalScale, transitionTime));

        // swap sprite back
        if (normalSprite != null) sr.sprite = normalSprite;

        isTransformed = false;
        isTransitioning = false;
        OnTransformedChanged?.Invoke(false);

        // restore speed if we changed it
        if (movement != null)
        {
            movement.SetSpeedMultiplier(1f);
        }

        if (animator != null) animator.SetTrigger("SetHuman");
    }

    IEnumerator Co_AutoRevert(float delay)
    {
        float t = 0f;
        while (t < delay)
        {
            if (!isTransformed) yield break; // if already reverted
            t += Time.deltaTime;
            yield return null;
        }
        StartTransformOut();
    }

    // Replaces Co_AutoRevert with visible countdown
    IEnumerator Co_AutoRevertWithCountdown(float totalSeconds)
    {
        // make sure countdown UI is visible
        SetCountdownVisible(true);

        float remaining = totalSeconds;
        int lastShown = -1;

        while (remaining > 0f)
        {
            if (!isTransformed) { SetCountdownVisible(false); yield break; }

            // Ceil to int for 5..1 display
            int toShow = Mathf.CeilToInt(remaining);

            if (toShow != lastShown)
            {
                SetCountdownText(toShow.ToString());
                lastShown = toShow;
            }

            remaining -= Time.deltaTime;
            yield return null;
        }

        // final tick to 0 just before revert (brief)
        SetCountdownText("0");
        yield return null;

        // hide + revert
        SetCountdownVisible(false);
        StartTransformOut();
    }

    private IEnumerator ScaleOverTime(Transform obj, Vector3 from, Vector3 to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            obj.localScale = Vector3.Lerp(from, to, t / time);
            yield return null;
        }
        obj.localScale = to;
    }

    // Public getter for stealth state
    public bool IsTransformed() => isTransformed;

    // ---------------- Countdown helpers ----------------

    void SetCountdownText(string s)
    {
        if (countdownTMP != null) countdownTMP.text = s;
        if (countdownText != null) countdownText.text = s;
    }

    void SetCountdownVisible(bool visible, bool instant = false)
    {
        // prefer TMP; fall back to Legacy
        if (countdownTMP != null)
        {
            var cg = GetOrAddCanvasGroup(countdownTMP.gameObject);
            if (fadeCountdown && !instant)
            {
                StartFade(cg, visible ? 1f : 0f, countdownFadeTime);
            }
            else
            {
                if (countdownFadeRoutine != null) StopCoroutine(countdownFadeRoutine);
                cg.alpha = visible ? 1f : 0f;
            }
            countdownTMP.gameObject.SetActive(true); // keep active; alpha controls visibility
        }
        else if (countdownText != null)
        {
            var cg = GetOrAddCanvasGroup(countdownText.gameObject);
            if (fadeCountdown && !instant)
            {
                StartFade(cg, visible ? 1f : 0f, countdownFadeTime);
            }
            else
            {
                if (countdownFadeRoutine != null) StopCoroutine(countdownFadeRoutine);
                cg.alpha = visible ? 1f : 0f;
            }
            countdownText.gameObject.SetActive(true);
        }
        // If neither assigned, do nothing (no UI)
    }

    CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    void StartFade(CanvasGroup cg, float target, float time)
    {
        if (countdownFadeRoutine != null) StopCoroutine(countdownFadeRoutine);
        countdownFadeRoutine = StartCoroutine(FadeRoutine(cg, target, time));
    }

    IEnumerator FadeRoutine(CanvasGroup cg, float target, float time)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime; // UI feels snappy even if timescale changes
            cg.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }
        cg.alpha = target;
    }
}
