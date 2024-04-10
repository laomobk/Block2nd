using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace TitlePage
{
    public class TitleImage : MonoBehaviour, IPointerDownHandler
    {
        private int time = 0;
        private RectTransform rectTransform;

        public void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            
            if (++time >= 10)
            {
                SceneManager.LoadScene("SOS");
            }

            rectTransform.localScale = (1 + time / 50f) * rectTransform.localScale;
            Debug.Log("click : " + time + "/10");
        }
    }
}