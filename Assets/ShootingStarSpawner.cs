using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShootingStarSpawner : MonoBehaviour
{
    [Header("Prefab (must be a Prefab asset with SpriteRenderer)")]
    public GameObject shootingStarPrefab;

    [Header("Spawn Control")]
    public int maxActiveStars = 1;
    public float spawnInterval = 7f;   // fixed delay

    [Header("Path / Motion")]
    public float speedMin = 16f;
    public float speedMax = 24f;
    public bool bothDirections = true;
    public float offscreenPadding = 1.0f;
    public float zDepth = 0f;

    [Header("Y Bands (fractions of camera height)")]
    public Vector2 spawnYBand = new Vector2(0.70f, 0.92f);
    public Vector2 endYBand = new Vector2(0.08f, 0.35f);

    [Header("Appearance")]
    public bool randomizeScale = true;
    public Vector2 scaleRange = new Vector2(0.6f, 1.1f);
    public string sortingLayerName = "Background";
    public int sortingOrder = 20;

    readonly List<GameObject> active = new List<GameObject>();
    Camera cam;
    Coroutine loop;

    void Awake()
    {
        cam = Camera.main;
    }

    void OnEnable()
    {
        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
        loop = null;
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f); // small delay for first star

        while (true)
        {
            // cleanup missing stars
            active.RemoveAll(go => go == null);

            if (active.Count < maxActiveStars)
                SpawnOnce();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void GetCameraWorldBounds(out float xLeft, out float xRight, out float yBottom, out float yTop)
    {
        Vector3 c = cam.transform.position;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        xLeft = c.x - halfW;
        xRight = c.x + halfW;
        yBottom = c.y - halfH;
        yTop = c.y + halfH;
    }

    void SpawnOnce()
    {
        if (!shootingStarPrefab)
        {
            Debug.LogError("[Spawner] Prefab reference is missing!");
            return;
        }

        GetCameraWorldBounds(out float xL, out float xR, out float yB, out float yT);

        float h = yT - yB;
        float startY = yB + Random.Range(spawnYBand.x, spawnYBand.y) * h;
        float endY = yB + Random.Range(endYBand.x, endYBand.y) * h;

        bool l2r = bothDirections ? (Random.value > 0.5f) : true;

        Vector3 start = l2r
            ? new Vector3(xL - offscreenPadding, startY, zDepth)
            : new Vector3(xR + offscreenPadding, startY, zDepth);
        Vector3 end = l2r
            ? new Vector3(xR + offscreenPadding, endY, zDepth)
            : new Vector3(xL - offscreenPadding, endY, zDepth);

        GameObject star = Instantiate(shootingStarPrefab, start, Quaternion.identity);

        // fix: ensure this stays alive and not override prefab reference
        star.name = "ShootingStarInstance";

        // sorting
        var sr = star.GetComponent<SpriteRenderer>();
        if (sr) { sr.sortingLayerName = sortingLayerName; sr.sortingOrder = sortingOrder; }

        // random scale
        if (randomizeScale)
        {
            float s = Random.Range(scaleRange.x, scaleRange.y);
            star.transform.localScale *= s;
        }

        // add + init ShootingStar
        var mover = star.AddComponent<ShootingStar>();
        mover.sortingLayerName = sortingLayerName;
        mover.sortingOrder = sortingOrder;
        mover.maxLifetime = 15f;
        mover.Init(end, Random.Range(speedMin, speedMax));

        active.Add(star);

        Debug.Log($"[Spawner] Spawned star at {start} → next in {spawnInterval}s (active={active.Count})");
    }
}
