using System;
using Block2nd.Client;
using Block2nd.Database;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class InfJumpButton : MonoBehaviour
    {
        private GameSettings gameSettings;
        private Text buttonText;

        private void Awake()
        {
            gameSettings = GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>().gameSettings;
            buttonText = transform.GetChild(0).gameObject.GetComponent<Text>();
        }

        private void Start()
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            buttonText.text = "Infinite Jump: " + (gameSettings.infiniteJump ? "On" : "Off");
        }

        public void Toggle()
        {
            gameSettings.infiniteJump = !gameSettings.infiniteJump;
            UpdateLabel();
        }
    }
}