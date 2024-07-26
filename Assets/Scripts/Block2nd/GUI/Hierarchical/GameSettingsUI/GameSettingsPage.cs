using System;
using Block2nd.Client;
using Block2nd.MathUtil;
using Block2nd.Scriptable;
using Block2nd.World;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.GameSettingsUI
{
    public class GameSettingsPage : MonoBehaviour
    {
        public readonly int[] viewDistanceCandidates = new int[] {4, 8, 16};
        private readonly string[] viewDistanceLevels = {"Tiny", "Near", "Far"};
        public readonly string[] shaderLevels = {"Low", "Medium", "High"};

        public GameSettings gameSettings;

        public Button graphicButton;
        public Button viewDistanceButton;
        public Button musicButton;
        public Button backButton;
        public Button vSyncButton;
        public Button fullScreenButton;

        private bool needToRefreshRender;

        private void Start()
        {
            gameSettings.LoadSettings();
            SyncWithSettings();
            needToRefreshRender = false;
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
            GetButtonText(vSyncButton).text = "V-Sync: " + (gameSettings.vSync ? "On" : "Off");
            GetButtonText(fullScreenButton).text = "Fullscreen: " + (gameSettings.fullScreen ? "On" : "Off");

            if (!Application.isMobilePlatform)
            {
                var res = Screen.resolutions[Screen.resolutions.Length - 1];
                if (gameSettings.fullScreen && Screen.fullScreenMode == FullScreenMode.Windowed)
                {
                    Screen.SetResolution(res.width, res.height, FullScreenMode.ExclusiveFullScreen);
                }

                if (!gameSettings.fullScreen && Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
                {
					Screen.SetResolution(res.width, res.height, FullScreenMode.Windowed);
                }
            }
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

            needToRefreshRender = true;
            GetButtonText(backButton).text = "Save And Back";
        }

        public void OnMusicButtonClick()
        {
            gameSettings.music = !gameSettings.music;
            gameSettings.dirty = true;
            SyncWithSettings();
            StoreSettings();
        }

        public void ToggleDebugUi()
        {
            GameClient.Instance.GuiCanvasManager.ToggleDebugUI();
        }

        public void ToggleVSync()
        {
            gameSettings.vSync = !gameSettings.vSync;
            gameSettings.dirty = true;
            SyncWithSettings();
            StoreSettings();
        }

        public void ToggleFullscreen()
        {
            gameSettings.fullScreen = !gameSettings.fullScreen;
            gameSettings.dirty = true;
            SyncWithSettings();
            StoreSettings();
        }

        public void OnExit()
        {
            if (GameClient.Instance == null)
                return;
            
            if (needToRefreshRender && GameClient.Instance.TryGetCurrentLevel(out Level level))
            {
                level.RefreshChunkRender();
            }
        }
    }
}