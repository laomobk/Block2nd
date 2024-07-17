using System;
using Block2nd.Client;
using Block2nd.MathUtil;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class ViewDistanceButton : MonoBehaviour
    {
        private readonly string[] levels = {"Tiny", "Near", "Far"};

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
            var levelIdx = MathHelper.FloorToLevelIndex(
                gameClient.gameSettings.viewDistance, 
                gameClient.viewDistanceCandidates);
            
            var level = levels[levelIdx];
            buttonText.text = "View Distance: " + level;
        }
    }
}