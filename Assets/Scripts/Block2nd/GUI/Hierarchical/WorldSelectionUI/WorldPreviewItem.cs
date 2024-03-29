using Block2nd.GameSave;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.WorldSelectionUI
{
    public class WorldPreviewItem : MonoBehaviour, IPointerDownHandler
    {
        private Text worldTitle;
        private Text worldSubtitle;
        private Image selectBox;

        private LevelSavePreview preview;
        private WorldListManager manager;

        public LevelSavePreview Preview => preview;

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

        public void Setup(LevelSavePreview preview, WorldListManager manager)
        {
            var accessTime = preview.lastWriteTime;
            var timeString = $" ({accessTime.Year}-{accessTime.Month}-{accessTime.Day} {accessTime.Hour:00}:{accessTime.Minute:00})";

            worldTitle.text = preview.name;
            worldSubtitle.text = preview.folderName + timeString;
            
            this.preview = preview;
            this.manager = manager;
        }

        public void SetHighlightState(bool state)
        {
            selectBox.enabled = state;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            manager.OnItemSelect(this);
        }
    }
}