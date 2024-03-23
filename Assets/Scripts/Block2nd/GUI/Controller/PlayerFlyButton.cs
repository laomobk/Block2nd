using System;
using Block2nd.GamePlay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Block2nd.GUI.Controller
{
    public class PlayerFlyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private PlayerController playerController;
        private bool inPress;

        public float flySpeed = 5f;

        private void Awake()
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (inPress)
            {
                playerController.flySpeed = flySpeed;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            inPress = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inPress = false;
            playerController.flySpeed = 0;
        }

        public void Reset()
        {
            inPress = false;
        }
    }
}