using System;
using Block2nd.MathUtil;
using Block2nd.Scriptable;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.GameSettingsUI
{
    public class GameSettingsPage : MonoBehaviour
    {
        public readonly int[] viewDistanceCandidates = new int[]{4, 8, 16, 32};
        private readonly string[] viewDistanceLevels = {"Tiny", "Near", "Far", "Very Far"};
        public readonly string[] shaderLevels = {"Low", "Medium", "High"};
        
        public GameSettings gameSettings;
        
        public Button graphicButton;
        public Button viewDistanceButton;
        public Button musicButton;

        private void Start()
        {
            gameSettings.LoadSettings();
            SyncWithSettings();
        }

        private Text GetButtonText(Button button)
        {
            return button.transform.GetChild(0).GetComponent<Text>();
        }

        private void SyncWithSettings()
        {
            var shader = gameSettings.shader;
            if (shader < shaderLevels.Length && shader >= 0)
            {
                GetButtonText(graphicButton).text = "Graphic Quality: " + shaderLevels[shader];
            }
            
            var levelIdx = MathHelper.FloorToLevelIndex(
                gameSettings.viewDistance, 
                viewDistanceCandidates);
            
            GetButtonText(viewDistanceButton).text = "View Distance: " + viewDistanceLevels[levelIdx];

            GetButtonText(musicButton).text = "Music: " + (gameSettings.music ? "On" : "Off");
        }

        private void StoreSettings()
        {
            gameSettings.StoreSettings();
        }

        public void OnShaderButtonClick()
        {
            gameSettings.shader = (gameSettings.shader + 1) % 3;
            SyncWithSettings();
            StoreSettings();
            gameSettings.dirty = true;
        }

        public void OnViewDistanceClick()
        {
            int i;
            int length = viewDistanceCandidates.Length;

            for (i = 0;
                i < length && gameSettings.viewDistance != viewDistanceCandidates[i];
                ++i)
            {
            }
            
            gameSettings.dirty = true;
            
            gameSettings.viewDistance = viewDistanceCandidates[(i + 1) % length];
            SyncWithSettings();
            StoreSettings();
        }

        public void OnMusicButtonClick()
        {
            gameSettings.music = !gameSettings.music;
            gameSettings.dirty = true;
            SyncWithSettings();
            StoreSettings();
        }
    }
}