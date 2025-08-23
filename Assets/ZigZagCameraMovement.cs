using UnityEngine;
using UnityEngine.SceneManagement;

public class ZigZagCameraMovement : MonoBehaviour
{
    public Transform[] points;
    public float moveSpeed = 2f;
    public float waitTimeAtEachPoint = 1f;
    public string nextSceneName = "GameScene";

    private int currentPointIndex = 0;
    private bool isWaiting = false;
    private float lockedZ;

    void Start()
    {
        lockedZ = transform.position.z; // Save original Z (e.g., -10 for 2D camera)
    }

    void Update()
    {
        if (isWaiting || currentPointIndex >= points.Length) return;

        Transform target = points[currentPointIndex];
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, lockedZ); // lock Z

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            StartCoroutine(WaitThenAdvance());
        }
    }

    System.Collections.IEnumerator WaitThenAdvance()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtEachPoint);
        currentPointIndex++;

        if (currentPointIndex >= points.Length)
        {
            SceneManager.LoadScene(nextSceneName);
        }

        isWaiting = false;
    }
}
