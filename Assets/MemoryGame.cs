using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryGame : MonoBehaviour
{
    public GameObject[] dots; // Assign your 4 existing dots in Inspector
    private List<int> sequence = new List<int>();
    private List<int> playerInput = new List<int>();

    private bool isPlayingSequence = false;
    private bool inputEnabled = false;

    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    void Start()
    {
        HideDots();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isPlayingSequence)
        {
            StartCoroutine(PlayMemoryGame());
        }
    }

    IEnumerator PlayMemoryGame()
    {
        // Show the panel/dots
        ShowDots();

        sequence.Clear();
        playerInput.Clear();
        isPlayingSequence = true;
        inputEnabled = false;

        // Generate random sequence of 4 dots
        for (int i = 0; i < dots.Length; i++)
        {
            int randomIndex = Random.Range(0, dots.Length);
            sequence.Add(randomIndex);
        }

        // Flash the dots in sequence
        foreach (int index in sequence)
        {
            HighlightDot(index);
            yield return new WaitForSeconds(0.5f);
            ResetDot(index);
            yield return new WaitForSeconds(0.3f);
        }

        isPlayingSequence = false;
        inputEnabled = true;
    }

    public void DotClicked(int index)
    {
        if (!inputEnabled) return;

        playerInput.Add(index);

        // Flash clicked dot briefly
        HighlightDot(index);
        StartCoroutine(ResetAfterDelay(index));

        // Check if the input is correct so far
        for (int i = 0; i < playerInput.Count; i++)
        {
            if (playerInput[i] != sequence[i])
            {
                Debug.Log("Wrong! Try again.");
                HideDots();
                return;
            }
        }

        // If full correct sequence
        if (playerInput.Count == sequence.Count)
        {
            Debug.Log("Success!");
            HideDots();
        }
    }

    IEnumerator ResetAfterDelay(int index)
    {
        yield return new WaitForSeconds(0.2f);
        ResetDot(index);
    }

    void HighlightDot(int index)
    {
        dots[index].GetComponent<SpriteRenderer>().color = highlightColor;
    }

    void ResetDot(int index)
    {
        dots[index].GetComponent<SpriteRenderer>().color = normalColor;
    }

    void ShowDots()
    {
        foreach (GameObject dot in dots)
            dot.SetActive(true);
    }

    void HideDots()
    {
        foreach (GameObject dot in dots)
            dot.SetActive(false);
    }
}
