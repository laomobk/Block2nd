using System;
using Block2nd.GamePlay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Block2nd.GUI.Controller
{
    public class PlayerFloatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private PlayerController playerController;
        private bool inPress;

        private void Awake()
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        private void Update()
        {
            playerController.SetFloatKeyState(inPress);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            inPress = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inPress = false;
        }

        public void Reset()
        {
            inPress = false;
        }
    }
}