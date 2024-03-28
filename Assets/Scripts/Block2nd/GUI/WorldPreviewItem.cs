using System;
using Block2nd.GameSave;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class WorldPreviewItem : MonoBehaviour
    {
        private Text worldTitle;
        private Text worldSubtitle;
        private Image selectBox;

        public string WorldTitle
        {
            set => worldTitle.text = value;
        }

        public string WorldSubtitle
        {
            set => worldSubtitle.text = value;
        }

        public bool SelectBox
        {
            set => selectBox.enabled = value;
        }

        public void Awake()
        {
            worldTitle = transform.Find("Title").GetComponent<Text>();
            worldSubtitle = transform.Find("Subtitle").GetComponent<Text>();
            selectBox = transform.Find("SelectBox").GetComponent<Image>();
        }

        private void Start()
        {
            selectBox.enabled = false; 
        }

        public void Setup(LevelSavePreview preview)
        {
            worldTitle.text = preview.name;
            worldSubtitle.text = preview.folderName;
        }
    }
}