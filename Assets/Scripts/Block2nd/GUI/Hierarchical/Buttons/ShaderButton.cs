using System;
using Block2nd.Client;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class ShaderButton : MonoBehaviour
    {
        private readonly string[] levels = {"Low", "Medium", "High"};

        private GameClient gameClient;
        private Text buttonText;

        private void Awake()
        {
            gameClient = GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>();
            buttonText = transform.GetChild(0).gameObject.GetComponent<Text>();
        }

        private void Start()
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            var level = levels[gameClient.ShaderCandidateIdx];
            buttonText.text = "Graphic Quality: " + level;
        }

        public void SwitchShader()
        {
            gameClient.SwitchShader();
            UpdateLabel();
        }
    }
}