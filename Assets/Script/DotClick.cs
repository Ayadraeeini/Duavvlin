using UnityEngine;

public class DotClick : MonoBehaviour
{
    public int dotIndex; // set this in Inspector
    public MemoryGame memoryGame; // drag MemoryGame object here

    private void OnMouseDown()
    {
        memoryGame.DotClicked(dotIndex);
    }
}
