using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class MemoryDot : MonoBehaviour
{
    private Image img;
    private Color originalColor;
    public Color flashColor = Color.yellow;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;


    private MemoryGameManager manager;

    void Start()
    {
        img = GetComponent<Image>();
        originalColor = img.color;
        manager = FindObjectOfType<MemoryGameManager>();
    }

    public IEnumerator FlashDot()
    {
        img.color = flashColor;
        yield return new WaitForSeconds(0.4f);
        img.color = originalColor;
    }

    public void HighlightCorrect()
    {
        img.color = correctColor;
    }

    public void HighlightWrong()
    {
        img.color = wrongColor;
    }

    public void ResetDot()
    {
        img.color = originalColor;
    }

    public void OnClick()
    {
        manager.DotClicked(this);
    }
}
