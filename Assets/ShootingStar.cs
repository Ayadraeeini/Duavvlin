using UnityEngine;

public class ShootingStar : MonoBehaviour
{
    Vector3 target;
    float speed;
    float life;

    [Header("Lifetime")]
    public float maxLifetime = 15f;

    [Header("Trail Look")]
    public float trailTime = 0.6f;
    public float startWidth = 0.22f;
    public float endWidth = 0f;
    public float hdrIntensity = 2.5f;

    [Header("Sorting")]
    public string sortingLayerName = "Background";
    public int sortingOrder = 20;

    TrailRenderer tr;
    SpriteRenderer sr;

    public void Init(Vector3 targetPos, float moveSpeed)
    {
        target = targetPos;
        speed = moveSpeed;
        life = 0f;

        if (!tr) SetupTrail();
        tr.Clear();
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr) { sr.sortingLayerName = sortingLayerName; sr.sortingOrder = sortingOrder; sr.color = Color.white; }
        SetupTrail();
    }

    void SetupTrail()
    {
        tr = GetComponent<TrailRenderer>();
        if (!tr) tr = gameObject.AddComponent<TrailRenderer>();

        tr.time = trailTime;
        tr.widthCurve = new AnimationCurve(
            new Keyframe(0f, startWidth),
            new Keyframe(1f, endWidth)
        );

        Color startHDR = Color.white * hdrIntensity;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(startHDR, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        tr.colorGradient = g;

        Shader add = Shader.Find("Sprites/Particles/Additive");
        tr.material = new Material(add ? add : Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        life += Time.deltaTime;

        if ((transform.position - target).sqrMagnitude < 0.05f || life >= maxLifetime)
            Destroy(gameObject);
    }
}
