using System;
using Block2nd.Client;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class ViewDistanceButton : MonoBehaviour
    {
        private readonly string[] levels = {"Tiny", "Near", "Far", "Very Far"};

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
            var level = levels[gameClient.ViewDistanceCandidateIdx];
            buttonText.text = "View Distance: " + level;
        }

        public void SwitchDistance()
        {
            gameClient.SwitchDistance();
            UpdateLabel();
        }
    }
}