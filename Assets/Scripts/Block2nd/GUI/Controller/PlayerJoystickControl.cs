using System;
using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.GUI.Controller
{
    [RequireComponent(typeof(JoyStick))]
    public class PlayerJoystickControl : MonoBehaviour
    {
        private PlayerController playerController;
        private JoyStick joyStick;

        private void Awake()
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            joyStick = GetComponent<JoyStick>();
        }

        private void Update()
        {
            playerController.MoveAxis(joyStick.Axis);
            playerController.SetRunState(joyStick.OverflowStick);
        }
    }
}