using UnityEngine;

public class FlashRedEvery2Seconds : MonoBehaviour
{
    [Header("Colors")]
    public Color colorA = Color.white;
    public Color colorB = Color.red;

    [Header("Flash Settings")]
    public float flashInterval = 2f;

    private SpriteRenderer sr;
    private MeshRenderer mr;
    private LineRenderer lr;

    private bool isRed = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mr = GetComponent<MeshRenderer>();
        lr = GetComponent<LineRenderer>();

        InvokeRepeating(nameof(ToggleColor), 0f, flashInterval);
    }

    void ToggleColor()
    {
        isRed = !isRed;
        ApplyColor(isRed ? colorB : colorA);
    }

    void ApplyColor(Color c)
    {
        if (sr) sr.color = c;

        if (lr)
        {
            lr.startColor = c;
            lr.endColor = c;
            if (lr.material) SetMatColor(lr.material, c);
        }

        if (mr && mr.material) SetMatColor(mr.material, c);
    }

    void SetMatColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }
}
