using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class MemoryUsageText : MonoBehaviour
    {
        private Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        private void Start()
        {
            StartCoroutine(UpdateCoroutine());
        }

        private IEnumerator UpdateCoroutine()
        {
            for (;;)
            {
                var used = Profiler.GetMonoUsedSizeLong();
                var total = Profiler.GetMonoHeapSizeLong();
                
                text.text = $"Used Memory: {(int)((double)used / total * 100f)}% ({used / 1024 / 1024}MB) of {total / 1024 / 1024}MB";
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
}