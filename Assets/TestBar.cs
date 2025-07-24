using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestBar : MonoBehaviour
{
    public Image image; // Assign this in the Inspector
    public float increaseSpeed = 0.5f; // Speed at which opacity increases

    public bool Touching;

    void Update()
    {
        if (Input.GetKey(KeyCode.E) && Touching == true)
        {
            // Increase the alpha value
            Color currentColor = image.color;
            currentColor.a += increaseSpeed * Time.deltaTime;
            image.color = currentColor;
        }
    }

    void OnTriggerEnter2D()
    {
        Touching = true;
    }

    void OnTriggerExit2D()
    {
        Touching = false;
    }
}