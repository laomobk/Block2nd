using System;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class WorldGeneratingProgressUI : MonoBehaviour
    {
        private Text progressText;
        private Text titleText;

        private void Awake()
        {
            progressText = transform.Find("ProgressText").GetComponent<Text>();
            titleText = transform.Find("TitleText").GetComponent<Text>();
        }

        public void SetProgress(float progress)
        {
            progressText.text = (int) (progress * 100) + "%";
        }
        
        public void SetProgress(string text)
        {
            progressText.text = text;
        }

        public void SetTitle(string text)
        {
            titleText.text = text;
        }
    }
}