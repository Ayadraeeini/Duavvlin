using UnityEngine;
using System.Collections;

public class ImportedFireworksTrigger2D : MonoBehaviour
{
    [Header("Who can trigger")]
    public string playerTag = "Player";

    [Header("Drag your imported firework prefabs here (1 or more)")]
    public GameObject[] fireworkPrefabs;

    [Header("Spawn placement")]
    [Tooltip("Offset from this trigger (e.g., up into the sky).")]
    public Vector2 spawnOffset = new Vector2(0f, 6f);

    [Tooltip("If miniShow is ON, spawn randomly inside this area centered at (trigger + offset).")]
    public Vector2 areaSize = new Vector2(12f, 4f);

    [Header("Show settings")]
    public bool miniShow = true;       // false = one burst, true = several bursts
    public int bursts = 6;
    public Vector2 delayRange = new Vector2(0.15f, 0.4f);

    [Header("Layering & Depth")]
    [Tooltip("Sorting Layer to force on all ParticleSystemRenderers.")]
    public string sortingLayerName = "Background";
    public int sortingOrder = -10;

    [Tooltip("Absolute world Z to place the fireworks (you asked for 60).")]
    public float zDepth = 60f;

    [Header("Trigger behavior")]
    public bool oneShot = true;        // fire once per run
    private bool _fired;

    [Header("Sound Settings")]
    public AudioClip[] fireworkSounds;         // Optional: multiple clips
    public float soundVolume = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_fired && oneShot) return;
        if (!other.CompareTag(playerTag)) return;

        if (fireworkPrefabs == null || fireworkPrefabs.Length == 0)
        {
            Debug.LogError("[ImportedFireworksTrigger2D] Assign 1+ fireworkPrefabs from the asset.");
            return;
        }

        if (miniShow)
            StartCoroutine(ShowRoutine());
        else
            SpawnOne(GetSpawnCenter());

        _fired = true;
    }

    IEnumerator ShowRoutine()
    {
        Vector3 center = GetSpawnCenter();
        Vector2 half = areaSize * 0.5f;

        for (int i = 0; i < bursts; i++)
        {
            Vector3 pos = center + new Vector3(
                Random.Range(-half.x, half.x),
                Random.Range(-half.y, half.y),
                0f
            );

            SpawnOne(pos);
            yield return new WaitForSeconds(Random.Range(delayRange.x, delayRange.y));
        }
    }

    Vector3 GetSpawnCenter()
    {
        return transform.position + (Vector3)spawnOffset;
    }

    void SpawnOne(Vector3 worldPos)
    {
        // pick a random prefab from your imported set
        var prefab = fireworkPrefabs[Random.Range(0, fireworkPrefabs.Length)];
        var go = Instantiate(prefab, worldPos, Quaternion.identity);

        // >>> Force Z = 60 (your request) <<<
        SetWorldZRecursively(go.transform, zDepth);

        // Play firework sound at spawn location
        PlayFireworkSound(worldPos);

        // Ensure all child particle systems render in the background
        foreach (var rend in go.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            rend.sortingLayerName = sortingLayerName;
            rend.sortingOrder = sortingOrder;
        }

        // Safety auto-destroy after longest particle lifetime
        float killAfter = ComputeMaxLifetime(go) + 1.0f;
        Destroy(go, killAfter);
    }

    void PlayFireworkSound(Vector3 worldPos)
    {
        if (fireworkSounds != null && fireworkSounds.Length > 0)
        {
            var clip = fireworkSounds[Random.Range(0, fireworkSounds.Length)];
            AudioSource.PlayClipAtPoint(clip, new Vector3(worldPos.x, worldPos.y, Camera.main.transform.position.z), soundVolume);
        }
    }

    void SetWorldZRecursively(Transform root, float z)
    {
        // Set the root first
        var p = root.position;
        p.z = z;
        root.position = p;

        // Also pin every child to the same Z to avoid any front-layer stragglers
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            var tp = t.position;
            tp.z = z;
            t.position = tp;
        }
    }

    float ComputeMaxLifetime(GameObject root)
    {
        float maxLife = 3f; // fallback
        var systems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < systems.Length; i++)
        {
            var ps = systems[i];
            var main = ps.main;
            float life = main.duration + main.startLifetime.constantMax;
            if (life > maxLife) maxLife = life;
        }
        return maxLife;
    }
}
