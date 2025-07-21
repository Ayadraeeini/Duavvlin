using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryGameManager : MonoBehaviour
{
    public GameObject panel;
    public List<MemoryDot> dots = new List<MemoryDot>();
    public int sequenceLength = 4;

    private List<MemoryDot> sequence = new List<MemoryDot>();
    private int currentIndex = 0;
    private bool gameActive = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !panel.activeInHierarchy)
        {
            StartCoroutine(StartMemoryGame());
        }
    }

    IEnumerator StartMemoryGame()
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        GenerateSequence();
        yield return StartCoroutine(ShowSequence());

        gameActive = true;
        currentIndex = 0;
    }

    void GenerateSequence()
    {
        sequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            var randomDot = dots[Random.Range(0, dots.Count)];
            sequence.Add(randomDot);
        }
    }

    IEnumerator ShowSequence()
    {
        foreach (var dot in sequence)
        {
            yield return dot.StartCoroutine(dot.FlashDot());
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void DotClicked(MemoryDot dot)
    {
        if (!gameActive) return;

        if (dot == sequence[currentIndex])
        {
            dot.HighlightCorrect();
            currentIndex++;

            if (currentIndex >= sequence.Count)
            {
                StartCoroutine(WinGame());
            }
        }
        else
        {
            StartCoroutine(LoseGame());
        }
    }

    IEnumerator WinGame()
    {
        gameActive = false;
        yield return new WaitForSeconds(0.5f);
        HideDots();
        panel.SetActive(false);
    }

    IEnumerator LoseGame()
    {
        gameActive = false;
        foreach (var dot in dots)
        {
            dot.HighlightWrong();
        }
        yield return new WaitForSeconds(1f);
        HideDots();
        panel.SetActive(false);
    }

    void HideDots()
    {
        foreach (var dot in dots)
        {
            dot.ResetDot();
        }
    }
}
