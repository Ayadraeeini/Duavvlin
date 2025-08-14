using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class SprayBox : MonoBehaviour
{
    // ====== UI/Art Setup ======
    [Header("Resource Names (Resources/...)")]
    [Tooltip("Resources/outline1.png → use 'outline1'")]
    public string outlineImageName = "outline1";
    [Tooltip("Resources/final1.png → use 'final1'")]
    public string finalImageName = "final1";

    [Header("Child Sprite Scales")]
    public Vector3 outlineScale = new Vector3(0.48f, 0.396f, 0.52f);
    public Vector3 finalScale = new Vector3(0.81f, 0.66f, 0.87f);

    [Header("Completion VFX")]
    public GameObject completionEffectPrefab;

    // ====== Audio ======
    public enum SoundMode { StagedClips, SingleClipWithPitch }

    [Header("Audio")]
    public SoundMode soundMode = SoundMode.StagedClips;
    public AudioSource audioSource;             // If null, auto-added on Start
    [Range(0f, 1f)] public float baseVolume = 0.9f;

    [Tooltip("Used when SoundMode = StagedClips. E.g. 4 clips increasing in intensity.")]
    public AudioClip[] stageClips;
    [Tooltip("Played exactly on completion (overrides others).")]
    public AudioClip finalClip;

    [Tooltip("Used when SoundMode = SingleClipWithPitch.")]
    public AudioClip singleSprayClip;
    [Tooltip("Pitch range for SingleClipWithPitch mode.")]
    public Vector2 pitchRange = new Vector2(1.0f, 1.5f);

    // ====== Progress ======
    [Header("Progress (read-only at runtime)")]
    [SerializeField] private int outlinePresses = 0;
    [SerializeField] private int finalPresses = 0;
    [SerializeField] private bool isCompleted = false;
    public bool IsCompleted => isCompleted;

    [Tooltip("Number of presses needed to fill outline, then final.")]
    public int maxPresses = 4;

    // ====== Player Proximity ======
    [Header("Player Proximity")]
    [Tooltip("Require player be inside trigger to accept 'E'.")]
    public bool requirePlayerInRange = true;
    private bool isPlayerInRange = false;

    // ====== Internals ======
    private SpriteRenderer baseRenderer;
    private Collider2D boxCollider;
    private SpriteRenderer outlineRenderer;
    private SpriteRenderer finalRenderer;

    // ====== Global completion event (PopularityMeter listens) ======
    public static System.Action<SprayBox> OnAnySprayCompleted;

    // -------------------------- Unity Lifecycle --------------------------
    void Awake()
    {
        baseRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        // Load textures
        Texture2D outlineTex = Resources.Load<Texture2D>(outlineImageName);
        Texture2D finalTex = Resources.Load<Texture2D>(finalImageName);

        if (outlineTex == null || finalTex == null)
        {
            Debug.LogError($"[SprayBox:{name}] Missing textures. Check Resources path names.");
            return;
        }

        // Create Outline child
        GameObject outlineGO = new GameObject("Outline");
        outlineGO.transform.SetParent(transform);
        outlineGO.transform.localPosition = Vector3.zero;
        outlineGO.transform.localScale = outlineScale;
        outlineRenderer = outlineGO.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = Sprite.Create(outlineTex, new Rect(0, 0, outlineTex.width, outlineTex.height), new Vector2(0.5f, 0.5f));
        outlineRenderer.sortingLayerID = baseRenderer.sortingLayerID;
        outlineRenderer.sortingOrder = baseRenderer.sortingOrder + 1;
        outlineRenderer.color = new Color(1f, 1f, 1f, 0f);

        // Create Final child
        GameObject finalGO = new GameObject("Final");
        finalGO.transform.SetParent(transform);
        finalGO.transform.localPosition = Vector3.zero;
        finalGO.transform.localScale = finalScale;
        finalRenderer = finalGO.AddComponent<SpriteRenderer>();
        finalRenderer.sprite = Sprite.Create(finalTex, new Rect(0, 0, finalTex.width, finalTex.height), new Vector2(0.5f, 0.5f));
        finalRenderer.sortingLayerID = baseRenderer.sortingLayerID;
        finalRenderer.sortingOrder = baseRenderer.sortingOrder + 2;
        finalRenderer.color = new Color(1f, 1f, 1f, 0f);

        // AudioSource fallback
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }

        // Collider should be trigger to detect player entry
        if (!boxCollider.isTrigger)
            Debug.LogWarning($"[SprayBox:{name}] Collider2D is not a trigger. Consider enabling IsTrigger for proximity.");
    }

    void Update()
    {
        if (isCompleted) return;

        // Press E to paint if in range (or if range not required)
        if ((!requirePlayerInRange || isPlayerInRange) && Input.GetKeyDown(KeyCode.E))
        {
            Paint();
        }
    }

    // -------------------------- Trigger Proximity --------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!requirePlayerInRange) return;
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!requirePlayerInRange) return;
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }

    // -------------------------- Painting Logic --------------------------
    /// <summary>
    /// Call this once per press (either by Update 'E' or your PlayerInteraction).
    /// </summary>
    public void Paint()
    {
        if (outlineRenderer == null || finalRenderer == null) return;
        if (isCompleted) return;

        bool justCompleted = false;

        // Fill outline first, then final
        if (outlinePresses < maxPresses)
        {
            outlinePresses++;
            float a = outlinePresses / (float)maxPresses;
            outlineRenderer.color = new Color(1f, 1f, 1f, a);
        }
        else if (finalPresses < maxPresses)
        {
            finalPresses++;
            float a = finalPresses / (float)maxPresses;
            finalRenderer.color = new Color(1f, 1f, 1f, a);

            if (finalPresses == maxPresses)
                justCompleted = true;
        }

        float progress01 = GetProgress01();

        if (justCompleted)
        {
            CompleteSpray();
        }
        else
        {
            PlayProgressSound(progress01);
        }
    }

    private float GetProgress01()
    {
        float total = maxPresses * 2f;          // outline + final
        float current = outlinePresses + finalPresses;
        return Mathf.Clamp01(current / total);
    }

    private void CompleteSpray()
    {
        if (isCompleted) return;
        isCompleted = true;

        HideBaseBox();    // visuals + disable collider
        PlayFinalSound(); // strong finish

        // Notify listeners (e.g., PopularityMeter)
        OnAnySprayCompleted?.Invoke(this);
    }

    private void HideBaseBox()
    {
        if (baseRenderer != null) baseRenderer.enabled = false;
        if (boxCollider != null) boxCollider.enabled = false;

        if (completionEffectPrefab != null)
            Instantiate(completionEffectPrefab, transform.position, Quaternion.identity);

        Debug.Log($"[SprayBox:{name}] Graffiti COMPLETE → base box hidden, collider disabled.");
    }

    // -------------------------- Audio Helpers --------------------------
    private void PlayProgressSound(float progress01)
    {
        if (audioSource == null) return;

        float vol = baseVolume * Mathf.Lerp(0.75f, 1f, progress01);

        switch (soundMode)
        {
            case SoundMode.StagedClips:
                if (stageClips != null && stageClips.Length > 0)
                {
                    int idx = Mathf.Clamp(Mathf.FloorToInt(progress01 * stageClips.Length), 0, stageClips.Length - 1);
                    if (progress01 > 0.99f) idx = stageClips.Length - 1;

                    var clip = stageClips[idx];
                    if (clip != null) audioSource.PlayOneShot(clip, vol);
                }
                break;

            case SoundMode.SingleClipWithPitch:
                if (singleSprayClip != null)
                {
                    audioSource.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, progress01);
                    audioSource.PlayOneShot(singleSprayClip, vol);
                    audioSource.pitch = 1f; // reset
                }
                break;
        }
    }

    private void PlayFinalSound()
    {
        if (audioSource == null) return;

        if (finalClip != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(finalClip, 1f);
            return;
        }

        if (soundMode == SoundMode.StagedClips && stageClips != null && stageClips.Length > 0)
        {
            var last = stageClips[stageClips.Length - 1];
            if (last != null) audioSource.PlayOneShot(last, 1f);
        }
        else if (soundMode == SoundMode.SingleClipWithPitch && singleSprayClip != null)
        {
            audioSource.pitch = pitchRange.y;
            audioSource.PlayOneShot(singleSprayClip, 1f);
            audioSource.pitch = 1f;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (maxPresses < 1) maxPresses = 1;
        if (pitchRange.x > pitchRange.y) pitchRange = new Vector2(pitchRange.y, pitchRange.x);
    }
#endif
}
