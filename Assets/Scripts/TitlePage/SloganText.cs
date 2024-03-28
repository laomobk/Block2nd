using System;
using UnityEngine;
using UnityEngine.UI;

namespace TitlePage
{
    public class SloganText : MonoBehaviour
    {
        [Range(25, 100)] public float twinklingSpeed = 1;
        [Range(0, 1)] public float minSize = 0.45f;
        
        private Text text;
        private RectTransform rectTransform;

        private void Awake()
        {
            text = GetComponent<Text>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            rectTransform.localScale = Vector3.one * 
                                       (2 - (Mathf.Abs(minSize * Mathf.Sin(Time.time * twinklingSpeed / 10f)) + 1));
        }
    }
}