using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // Requires TextMeshPro package

public class RecognitionFlash : MonoBehaviour
{
    public static RecognitionFlash Instance;

    [Header("Assign ONE of these (or leave empty to auto-find on this object)")]
    public TextMeshProUGUI tmp; // TMP Text
    public Text legacyText;     // Legacy UI Text

    [Header("SFX (optional)")]
    public AudioSource sfxSource;
    public AudioClip[] sfxClips;

    [Header("Messages (popularity-focused)")]
    public string[] messages = {
        "POPULARITY UP!",
        "ALL EYES ON YOU!",
        "GETTING KNOWN!",
        "NAME SPREADING!",
        "CITY SEES YOU!",
        "RESPECT RISING!",
        "LEGEND GROWS!"
    };

    [Header("Timing")]
    public float punchInTime = 0.12f;
    public float holdTime = 0.55f;
    public float punchOutTime = 0.12f;
    public float fadeOutTime = 0.45f;

    [Header("Scale Punch (base)")]
    public float startScale = 1.00f;
    public float peakScale = 1.20f; // base peak (will be multiplied by popularity)
    public float endScale = 1.00f;

    [Header("Color Policy")]
    public bool usePaletteFirst = true;
    [Tooltip("Cool, high-contrast colors to avoid orange/pink/purple backgrounds.")]
    public Color[] palette = new Color[] {
        new Color(0.05f, 0.95f, 1.00f), // electric cyan
        new Color(0.20f, 0.90f, 0.25f), // neon green
        new Color(0.30f, 0.80f, 1.00f), // sky blue
        Color.white                      // clean white
    };

    [Tooltip("If palette is empty or disabled, generate cool hues via HSV (greens/cyans/blues).")]
    public bool useCoolHSV = true;

    [Header("TMP Style (applied at runtime)")]
    public bool autoStyleTMP = true;
    [Range(0f, 1f)] public float outlineWidth = 0.28f;
    public Color outlineColor = Color.black;
    public Color underlayColor = new Color(0f, 0f, 0f, 0.85f);
    public Vector2 underlayOffset = new Vector2(2f, -2f);
    [Range(0f, 1f)] public float underlaySoftness = 0.5f;

    [Header("Glow (neon)")]
    [Tooltip("Base glow power; actual glow scales up with popularity.")]
    [Range(0f, 1f)] public float baseGlowPower = 0.35f;
    [Range(0f, 1.5f)] public float maxExtraGlow = 0.55f; // added when popularity = 1

    // Internals
    CanvasGroup cg;
    RectTransform rt;

    void Awake()
    {
        Instance = this;

        rt = GetComponent<RectTransform>();
        if (!rt) rt = gameObject.AddComponent<RectTransform>();

        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();

        if (tmp == null) tmp = GetComponent<TextMeshProUGUI>();
        if (legacyText == null) legacyText = GetComponent<Text>();

        cg.alpha = 0f;
        gameObject.SetActive(true);

        if (tmp != null)
        {
            tmp.fontStyle |= FontStyles.Bold;
            if (autoStyleTMP) ApplyTMPStyle(tmp, tmp.color, baseGlowPower);
        }
        else if (legacyText != null)
        {
            legacyText.fontStyle = FontStyle.Bold;
        }
    }

    /// <summary>
    /// Show popup.
    /// message: optional custom text; if null/empty, a random one from the list.
    /// popularity01: optional 0..1 to scale size & glow with current popularity.
    /// </summary>
    public void ShowRecognition(string message = null, float popularity01 = -1f)
    {
        if (tmp == null && legacyText == null)
        {
            Debug.LogWarning("[RecognitionFlash] No Text component found/assigned (TMP or Legacy).");
            return;
        }

        // Clamp / default popularity for intensity
        float pop = Mathf.Clamp01(popularity01 < 0f ? 0.5f : popularity01);

        // Choose message
        if (string.IsNullOrEmpty(message))
        {
            if (messages != null && messages.Length > 0)
                message = messages[Random.Range(0, messages.Length)];
            else
                message = "POPULARITY UP!";
        }

        // Choose a cool, high-contrast color (avoid warm/pink/purple)
        var color = PickCoolColor();

        // Apply text + color
        if (tmp)
        {
            tmp.text = message;
            tmp.color = color;
            if (autoStyleTMP) ApplyTMPStyle(tmp, color, baseGlowPower + maxExtraGlow * pop);
        }
        if (legacyText)
        {
            legacyText.text = message;
            legacyText.color = color;
            legacyText.fontStyle = FontStyle.Bold;
        }

        // Optional SFX
        if (sfxSource && sfxClips != null && sfxClips.Length > 0)
        {
            var clip = sfxClips[Random.Range(0, sfxClips.Length)];
            if (clip) sfxSource.PlayOneShot(clip, 1f);
        }

        StopAllCoroutines();
        StartCoroutine(FlashRoutine(pop));
    }

    IEnumerator FlashRoutine(float popularity01)
    {
        // Popularity scales the punch a bit (up to +15%)
        float scaleBoost = Mathf.Lerp(1f, 1.15f, popularity01);
        float localPeak = peakScale * scaleBoost;

        cg.alpha = 1f;
        rt.localScale = Vector3.one * startScale;

        // PUNCH IN
        if (punchInTime > 0f)
        {
            float t = 0f;
            while (t < punchInTime)
            {
                t += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(startScale, localPeak, EaseOutCubic(t / punchInTime));
                rt.localScale = Vector3.one * s;
                yield return null;
            }
        }
        else rt.localScale = Vector3.one * localPeak;

        // HOLD
        if (holdTime > 0f) yield return new WaitForSecondsRealtime(holdTime);

        // PUNCH OUT
        if (punchOutTime > 0f)
        {
            float t = 0f;
            while (t < punchOutTime)
            {
                t += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(localPeak, endScale, EaseInCubic(t / punchOutTime));
                rt.localScale = Vector3.one * s;
                yield return null;
            }
        }
        else rt.localScale = Vector3.one * endScale;

        // FADE OUT
        if (fadeOutTime > 0f)
        {
            float t = 0f;
            while (t < fadeOutTime)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
                yield return null;
            }
        }
        cg.alpha = 0f;
    }

    // ---------- Helpers ----------
    float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - Mathf.Clamp01(x), 3f);
    float EaseInCubic(float x) => Mathf.Pow(Mathf.Clamp01(x), 3f);

    Color PickCoolColor()
    {
        if (usePaletteFirst && palette != null && palette.Length > 0)
            return palette[Random.Range(0, palette.Length)];

        if (useCoolHSV)
        {
            // Choose hue only from cool ranges:
            //   Greens ~110–160°, Cyans ~160–200°, Blues ~200–240°
            float[] hRanges = { 110f / 360f, 160f / 360f, 200f / 360f, 240f / 360f };
            float h;
            if (Random.value < 0.5f)
                h = Random.Range(hRanges[0], hRanges[1]); // green
            else if (Random.value < 0.5f)
                h = Random.Range(hRanges[1], hRanges[2]); // cyan
            else
                h = Random.Range(hRanges[2], hRanges[3]); // blue

            float s = Random.Range(0.9f, 1.0f); // high saturation
            float v = Random.Range(0.95f, 1.0f); // bright
            return Color.HSVToRGB(h, s, v);
        }

        return Color.white;
    }

    // Apply bold outline, shadow (underlay), and glow to TMP at runtime
    void ApplyTMPStyle(TextMeshProUGUI t, Color glowColor, float glowPower)
    {
        if (t == null) return;
        var mat = t.fontMaterial;
        if (mat == null) return;

        t.fontStyle |= FontStyles.Bold;

        // Outline
        if (mat.HasProperty(ShaderUtilities.ID_OutlineWidth))
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
        if (mat.HasProperty(ShaderUtilities.ID_OutlineColor))
            mat.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);

        // Underlay (shadow)
        if (mat.HasProperty(ShaderUtilities.ID_UnderlayColor))
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, underlayColor);
        if (mat.HasProperty(ShaderUtilities.ID_UnderlayOffsetX))
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, underlayOffset.x);
        if (mat.HasProperty(ShaderUtilities.ID_UnderlayOffsetY))
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, underlayOffset.y);
        if (mat.HasProperty(ShaderUtilities.ID_UnderlaySoftness))
            mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, underlaySoftness);

        // Glow (neon feel)
        if (mat.HasProperty(ShaderUtilities.ID_GlowColor))
            mat.SetColor(ShaderUtilities.ID_GlowColor, glowColor);
        if (mat.HasProperty(ShaderUtilities.ID_GlowPower))
            mat.SetFloat(ShaderUtilities.ID_GlowPower, Mathf.Clamp(glowPower, 0f, 1.5f));

        // Re-assign to force update
        t.fontMaterial = mat;
    }
}
