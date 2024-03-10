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

        StartCoroutine(FPSTextUpdateCoroutine());
    }

    private void Update()
    {
        fpsSum += (int) (1 / Time.deltaTime);
        count++;
    }

    private IEnumerator FPSTextUpdateCoroutine()
    {
        while (true)
        {
            if (count != 0)
            {
                text.text = fpsSum / count + " FPS";
                count = 0;
                fpsSum = 0;
            }

            yield return new WaitForSeconds(1f);
        }
    }
}