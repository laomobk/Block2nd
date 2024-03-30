using System;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class WorldGeneratingProgressUI : MonoBehaviour
    {
        private Text progressText;
        private Text titleText;
        private ProgressBar progressBar;

        private void Awake()
        {
            progressText = transform.Find("ProgressText").GetComponent<Text>();
            titleText = transform.Find("TitleText").GetComponent<Text>();
            progressBar = transform.Find("ProgressBar").GetComponent<ProgressBar>();
        }

        public void SetProgress(float progress)
        {
            progressBar.gameObject.SetActive(true); 
            progressText.gameObject.SetActive(false);
            
            progressBar.SetProgress(progress);
        }
        
        public void SetProgress(string text)
        {
            progressBar.gameObject.SetActive(false);
            progressText.gameObject.SetActive(true);
            
            progressText.text = text;
        }

        public void SetTitle(string text)
        {
            titleText.text = text;
        }
    }
}