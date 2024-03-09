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

        public Sprite normalStateLook;
        public Sprite hoveStateLook;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Start()
        {   
            image.sprite = normalStateLook;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            image.sprite = hoveStateLook;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            image.sprite = normalStateLook;
        }
    }
}