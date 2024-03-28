using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    [RequireComponent(typeof(Image))]
    public class ButtonStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Image image;
        private Text text;
        private Button button;

        public Sprite disabledStateLook;
        public Sprite normalStateLook;
        public Sprite hoveStateLook;

        private void Awake()
        {
            if (transform.childCount > 0)
                text = transform.Find("Text").GetComponent<Text>();
            
            button = GetComponent<Button>();
            image = GetComponent<Image>();
        }

        private void Start()
        {   
            image.sprite = normalStateLook;
        }

        private void Update()
        {
            if (!button.interactable && disabledStateLook != null)
            {
                image.sprite = disabledStateLook;
                if (text != null)
                {
                    text.color = new Color(0.6f, 0.6f, 0.6f);
                }
            }

            if (button.interactable && image.sprite == disabledStateLook)
            {
                image.sprite = normalStateLook;
                if (text != null)
                {
                    text.color = new Color(1f, 1f, 1f);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button.interactable)
                image.sprite = hoveStateLook;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (button.interactable)
                image.sprite = normalStateLook;
        }
    }
}