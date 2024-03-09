using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    [RequireComponent(typeof(Image))]
    public class HoverButtonStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Image image;

        public Color color;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Start()
        {
            color.a = 0.0f;
            image.color = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            color.a = 0.5f;
            image.color = color;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            color.a = 0;
            image.color = color;
        }
    }
}