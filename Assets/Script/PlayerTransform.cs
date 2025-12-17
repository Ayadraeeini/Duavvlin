using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;

public class PlayerTransform : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite transformedSprite;

    [Header("Controls")]
    public InputReader input; // 👈 New InputReader reference

    [Header("Timing")]
    public float transitionTime = 0.5f;
    public float maxTransformDuration = 5f;

    [Header("Duration Control")]
    public bool forceFullDuration = true;

    [Header("Scale")]
    public Vector3 canScale = new Vector3(0.5f, 0.5f, 1f);

    [Header("Movement While Transformed")]
    public bool allowMovementWhileTransformed = true;
    public float transformedSpeedMultiplier = 1f;

    [Header("Countdown UI (optional)")]
    public TextMeshProUGUI countdownTMP;
    public Text countdownText;
    public bool fadeCountdown = true;
    public float countdownFadeTime = 0.15f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip transformInSound;
    public AudioClip transformOutSound;

    private SpriteRenderer sr;
    private Animator animator;
    private Movement movement;
    private Vector3 originalScale;
    private bool isTransformed = false;
    private bool isTransitioning = false;

    private Coroutine activeTransition;
    private Coroutine autoRevertRoutine;
    private Coroutine countdownFadeRoutine;

    public event Action<bool> OnTransformedChanged;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        movement = GetComponent<Movement>();
        originalScale = transform.localScale;

        if (normalSprite != null) sr.sprite = normalSprite;
        if (animator == null) Debug.LogWarning("Animator is missing from Player!");
        if (input == null) Debug.LogWarning("InputReader reference is not assigned!");

        SetCountdownVisible(false, true);
        SetCountdownText("");
    }

    void Update()
    {
        if (input != null && input.TransformPressed())
        {
            ToggleTransform();
        }
    }

    public void ToggleTransform()
    {
        if (isTransitioning) return;
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

        if (audioSource != null && transformInSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(transformInSound);
        }

        if (animator != null) animator.SetTrigger("ToCan");
        yield return StartCoroutine(ScaleOverTime(transform, transform.localScale, canScale, transitionTime));

        isTransformed = true;
        isTransitioning = false;
        OnTransformedChanged?.Invoke(true);

        if (allowMovementWhileTransformed && movement != null)
        {
            movement.SetSpeedMultiplier(transformedSpeedMultiplier);
        }

        if (maxTransformDuration > 0f)
        {
            autoRevertRoutine = StartCoroutine(Co_AutoRevertWithCountdown(maxTransformDuration));
        }
    }

    IEnumerator Co_TransformOut()
    {
        isTransitioning = true;

        if (audioSource != null && transformOutSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(transformOutSound);
        }

        if (animator != null) animator.SetTrigger("ToHuman");
        SetCountdownVisible(false);

        yield return StartCoroutine(ScaleOverTime(transform, transform.localScale, originalScale, transitionTime));

        if (normalSprite != null) sr.sprite = normalSprite;

        isTransformed = false;
        isTransitioning = false;
        OnTransformedChanged?.Invoke(false);

        if (movement != null)
        {
            movement.SetSpeedMultiplier(1f);
        }

        if (animator != null) animator.SetTrigger("SetHuman");
    }

    IEnumerator Co_AutoRevertWithCountdown(float totalSeconds)
    {
        SetCountdownVisible(true);

        float remaining = totalSeconds;
        int lastShown = -1;

        while (remaining > 0f)
        {
            if (!isTransformed) { SetCountdownVisible(false); yield break; }

            int toShow = Mathf.CeilToInt(remaining);

            if (toShow != lastShown)
            {
                SetCountdownText(toShow.ToString());
                lastShown = toShow;
            }

            remaining -= Time.deltaTime;
            yield return null;
        }

        SetCountdownText("0");
        yield return null;

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

    public bool IsTransformed() => isTransformed;

    void SetCountdownText(string s)
    {
        if (countdownTMP != null) countdownTMP.text = s;
        if (countdownText != null) countdownText.text = s;
    }

    void SetCountdownVisible(bool visible, bool instant = false)
    {
        if (countdownTMP != null)
        {
            var cg = GetOrAddCanvasGroup(countdownTMP.gameObject);
            if (fadeCountdown && !instant)
                StartFade(cg, visible ? 1f : 0f, countdownFadeTime);
            else
            {
                if (countdownFadeRoutine != null) StopCoroutine(countdownFadeRoutine);
                cg.alpha = visible ? 1f : 0f;
            }
            countdownTMP.gameObject.SetActive(true);
        }
        else if (countdownText != null)
        {
            var cg = GetOrAddCanvasGroup(countdownText.gameObject);
            if (fadeCountdown && !instant)
                StartFade(cg, visible ? 1f : 0f, countdownFadeTime);
            else
            {
                if (countdownFadeRoutine != null) StopCoroutine(countdownFadeRoutine);
                cg.alpha = visible ? 1f : 0f;
            }
            countdownText.gameObject.SetActive(true);
        }
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
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }
        cg.alpha = target;
    }
}
