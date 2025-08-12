using UnityEngine;

public class SprayBox : MonoBehaviour
{
    [Header("Resource Names (Resources/...)")]
    public string outlineImageName = "outline1";
    public string finalImageName = "final1";

    [Header("Scales")]
    public Vector3 outlineScale = new Vector3(0.48f, 0.396f, 0.52f);
    public Vector3 finalScale = new Vector3(0.81f, 0.66f, 0.87f);

    [Header("Completion VFX")]
    public GameObject completionEffectPrefab;

    // --- Audio ---
    public enum SoundMode { StagedClips, SingleClipWithPitch }
    [Header("Audio")]
    public SoundMode soundMode = SoundMode.StagedClips;
    public AudioSource audioSource;           // If null, will be auto-created
    [Range(0f, 1f)] public float baseVolume = 0.9f;

    [Tooltip("Stages used when SoundMode = StagedClips (0..n-1), last stage will be near-complete. Optional Final clip below.")]
    public AudioClip[] stageClips;            // e.g., 4 clips for rising intensity
    [Tooltip("Played exactly on completion.")]
    public AudioClip finalClip;

    [Tooltip("Used when SoundMode = SingleClipWithPitch.")]
    public AudioClip singleSprayClip;
    [Tooltip("Pitch range for SingleClipWithPitch mode.")]
    public Vector2 pitchRange = new Vector2(1.0f, 1.5f);

    // --- Internals ---
    private SpriteRenderer outlineRenderer;
    private SpriteRenderer finalRenderer;
    private SpriteRenderer baseRenderer;
    private Collider2D boxCollider;

    private int outlinePresses = 0;
    private int finalPresses = 0;
    private const int maxPresses = 4; // 4 presses for outline, 4 presses for final

    private bool isPlayerInRange = false; // To track player proximity

    void Start()
    {
        // Base components
        baseRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<Collider2D>();

        // Load textures
        Texture2D outlineTex = Resources.Load<Texture2D>(outlineImageName);
        Texture2D finalTex = Resources.Load<Texture2D>(finalImageName);

        if (outlineTex == null || finalTex == null)
        {
            Debug.LogError("[SprayBox] Textures not found! Check Resources folder and file names.");
            return;
        }

        // Create Outline child
        GameObject outlineGO = new GameObject("Outline");
        outlineGO.transform.SetParent(transform);
        outlineGO.transform.localPosition = Vector3.zero;
        outlineGO.transform.localScale = outlineScale;
        outlineRenderer = outlineGO.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = Sprite.Create(outlineTex, new Rect(0, 0, outlineTex.width, outlineTex.height), new Vector2(0.5f, 0.5f));
        outlineRenderer.color = new Color(1f, 1f, 1f, 0f);

        // Create Final child
        GameObject finalGO = new GameObject("Final");
        finalGO.transform.SetParent(transform);
        finalGO.transform.localPosition = Vector3.zero;
        finalGO.transform.localScale = finalScale;
        finalRenderer = finalGO.AddComponent<SpriteRenderer>();
        finalRenderer.sprite = Sprite.Create(finalTex, new Rect(0, 0, finalTex.width, finalTex.height), new Vector2(0.5f, 0.5f));
        finalRenderer.color = new Color(1f, 1f, 1f, 0f);

        // AudioSource fallback
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D by default; set >0 if you want 3D audio
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E)) // Check if player is in range and pressed 'E'
        {
            Paint(); // Call the Paint function when 'E' is pressed
        }
    }

    // Trigger detection for the player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    /// <summary>
    /// Call this once per press.
    /// </summary>
    public void Paint()
    {
        if (outlineRenderer == null || finalRenderer == null) return;

        bool justCompleted = false;

        // First fill outline (0..4), then final (0..4)
        if (outlinePresses < maxPresses)
        {
            outlinePresses++;
            float alpha = outlinePresses / (float)maxPresses;
            outlineRenderer.color = new Color(1f, 1f, 1f, alpha);
        }
        else if (finalPresses < maxPresses)
        {
            finalPresses++;
            float alpha = finalPresses / (float)maxPresses;
            finalRenderer.color = new Color(1f, 1f, 1f, alpha);

            if (finalPresses == maxPresses)
            {
                justCompleted = true;
                HideBaseBox();
            }
        }

        // Play sound after updating progress so audio reflects the *new* state
        float progress = GetProgress01(); // 0..1 after this press

        if (justCompleted)
        {
            PlayFinalSound(); // force the final sound on completion
        }
        else
        {
            PlayProgressSound(progress);
        }
    }

    private float GetProgress01()
    {
        float total = maxPresses * 2f; // outline + final
        float current = outlinePresses + finalPresses;
        return Mathf.Clamp01(current / total);
    }

    private void PlayProgressSound(float progress01)
    {
        if (audioSource == null) return;

        // Subtle volume ramp as we approach completion
        float vol = baseVolume * Mathf.Lerp(0.75f, 1f, progress01);

        switch (soundMode)
        {
            case SoundMode.StagedClips:
                if (stageClips != null && stageClips.Length > 0)
                {
                    // Map progress 0..~0.999 to stage indices 0..(n-1)
                    int idx = Mathf.Clamp(Mathf.FloorToInt(progress01 * stageClips.Length), 0, stageClips.Length - 1);

                    // If we're extremely close to complete but not finished, prefer the last stage
                    if (progress01 > 0.99f) idx = stageClips.Length - 1;

                    AudioClip clip = stageClips[idx];
                    if (clip != null) audioSource.PlayOneShot(clip, vol);
                }
                break;

            case SoundMode.SingleClipWithPitch:
                if (singleSprayClip != null)
                {
                    float pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, progress01);
                    audioSource.pitch = pitch;
                    audioSource.PlayOneShot(singleSprayClip, vol);
                    // Reset pitch so it doesn't affect other sounds
                    audioSource.pitch = 1f;
                }
                break;
        }
    }

    private void PlayFinalSound()
    {
        if (audioSource == null) return;

        // Prefer explicit finalClip
        if (finalClip != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(finalClip, 1f);
            return;
        }

        // Fallbacks if no finalClip provided
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

    private void HideBaseBox()
    {
        if (baseRenderer != null) baseRenderer.enabled = false;
        if (boxCollider != null) boxCollider.enabled = false;

        Debug.Log("[SprayBox] Graffiti complete — base box hidden.");

        if (completionEffectPrefab != null)
        {
            Instantiate(completionEffectPrefab, transform.position, Quaternion.identity);
            Debug.Log("[SprayBox] Particle effect played.");
        }
    }
}
