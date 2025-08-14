using UnityEngine;
using System.Collections;
using System;

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
    public float maxTransformDuration = 5f; // exactly 5s per your request

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

        // swap sprite
        if (transformedSprite != null) sr.sprite = transformedSprite;

        isTransformed = true;
        isTransitioning = false;
        OnTransformedChanged?.Invoke(true);

        // movement stays enabled; optionally adjust speed
        if (allowMovementWhileTransformed && movement != null)
        {
            movement.SetSpeedMultiplier(transformedSpeedMultiplier); // Movement has this now
        }

        // auto revert (exact 5s unless forceFullDuration=false and user toggles early)
        if (maxTransformDuration > 0f)
        {
            autoRevertRoutine = StartCoroutine(Co_AutoRevert(maxTransformDuration));
        }
    }

    IEnumerator Co_TransformOut()
    {
        isTransitioning = true;

        if (animator != null) animator.SetTrigger("ToHuman");

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
    }

    IEnumerator Co_AutoRevert(float delay)
    {
        float t = 0f;
        while (t < delay)
        {
            // If early manual revert is allowed and already reverted, stop timer
            if (!isTransformed) yield break;
            t += Time.deltaTime;
            yield return null;
        }
        // time’s up → revert
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
}
