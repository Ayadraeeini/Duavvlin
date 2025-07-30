using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestBar : MonoBehaviour
{
    public ProgressBar script;

    public Image image; // Assign this in the Inspector
    public float increaseSpeed = 0.25f; // Speed at which opacity increases

    public bool Touching;

    void Update()
    {
        if (Input.GetKey(KeyCode.E) && Touching == true)
        {
            // Increase the alpha value
            Color currentColor = image.color;
            currentColor.a += increaseSpeed * Time.deltaTime;
            image.color = currentColor;
            script.IncrementProgress(increaseSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D()
    {
        Touching = true;
        script.SetProgress(image.color.a);
    }

    void OnTriggerExit2D()
    {
        Touching = false;
    }
}