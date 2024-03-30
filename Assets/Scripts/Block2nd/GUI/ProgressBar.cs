using System;
using UnityEngine;

namespace Block2nd.GUI
{
    public class ProgressBar : MonoBehaviour
    {
        public float width = 160;
        private RectTransform progressRectTransform;

        private void Awake()
        {
            progressRectTransform = transform.Find("Progress").GetComponent<RectTransform>();
        }

        public void SetProgress(float progress)
        {
            progressRectTransform.offsetMax = new Vector2(160 * progress - 160, 0);
        }
    }
}