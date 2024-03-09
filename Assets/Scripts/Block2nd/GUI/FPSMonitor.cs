using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSMonitor : MonoBehaviour
{
    private Text text;
    private int count;
    private int fpsSum;

    // Use this for initialization
    void Start()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        if (count > 10)
        {
            text.text = fpsSum / 10 + " FPS";
            count = 0;
            fpsSum = 0;
        }
        else
        {
            fpsSum += (int)(1 / Time.deltaTime);
        }

        count++;
    }
}