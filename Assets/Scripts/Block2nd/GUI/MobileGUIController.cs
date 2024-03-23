using System;
using Block2nd.GamePlay;
using Block2nd.GUI.Controller;
using UnityEngine;

namespace Block2nd.GUI
{
    public class MobileGUIController : MonoBehaviour
    {
        public GameObject floatButton;
        public GameObject flyButtons;
        
        private PlayerController playerController;

        private void Awake()
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }
        
        private void Update()
        {
            floatButton.SetActive(playerController.InWater && !playerController.flying);
            flyButtons.SetActive(playerController.flying);

            if (!playerController.InWater)
            {
                floatButton.GetComponent<PlayerFloatButton>().Reset();
                playerController.SetFloatKeyState(false);
            }
        }
    }
}