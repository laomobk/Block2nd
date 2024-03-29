using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.GameSave;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.WorldSelectionUI
{
    public class WorldSelectListPage : MonoBehaviour
    {
        public WorldListManager manager;

        public Button playSelectedWorldButton;
        public Button renameWorldButton;
        public Button deleteWorldButton;

        private void Start()
        {
            // CreateTestList();
            
            manager.onLevelSelectedAction = delegate(LevelSavePreview preview)
            {
                playSelectedWorldButton.interactable = true;
                renameWorldButton.interactable = true;
                deleteWorldButton.interactable = true;

                ClientSharedData.levelSavePreviewInLastContext = preview;
            };
            
            manager.onPlaySelectedWorldAction = PlaySelectedWorld;

            var previews = LevelSaveManager.GetAllLevelSavePreviews();
            if (previews == null)
            {
                return;
            }
            
            manager.SetPreviewList(previews);
        }

        private void CreateTestList()
        {
            var list = new List<LevelSavePreview>();

            list.Add(new LevelSavePreview
            {
                name = "Hello",
                folderName = "Klee"
            });
            
            list.Add(new LevelSavePreview
            {
                name = "Hello",
                folderName = "Klee"
            });

            list.Add(new LevelSavePreview
            {
                name = "Hello",
                folderName = "Klee"
            });

            manager.SetPreviewList(list);
        }

        public void PlaySelectedWorld(LevelSavePreview preview)
        {
            Debug.Log("Play world: " + preview.name);

            ClientSharedData.levelSavePreviewInLastContext = preview;

            SceneManager.LoadScene("Game");
        }
    }
}