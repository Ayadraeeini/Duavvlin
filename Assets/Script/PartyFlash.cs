using UnityEngine;

public class PartyFlash : MonoBehaviour
{
    private SpriteRenderer sr;

    private Color[] colors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.magenta,
        Color.yellow,
        Color.cyan,
        Color.white
    };

    private int index = 0;
    private float timer = 0f;
    public float changeRate = 1f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeRate)
        {
            index = (index + 1) % colors.Length;
            sr.color = colors[index];
            Debug.Log("Color changed to: " + colors[index]);
            timer = 0f;
        }
    }
}
