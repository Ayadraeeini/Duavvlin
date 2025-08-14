using UnityEngine;

public class WindSpawner : MonoBehaviour
{
    public GameObject paperPrefab;
    public float spawnInterval = 2f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            float y = Random.Range(-3f, 3f);
            Vector3 spawnPos = new Vector3(Camera.main.transform.position.x - 10f, y, 0);
            Instantiate(paperPrefab, spawnPos, Quaternion.identity);
        }
    }
}
