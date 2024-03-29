using System;
using System.Collections.Generic;
using Block2nd.GameSave;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class WorldSelectListUI : MonoBehaviour
    {
        public WorldListManager manager;

        public Button playSelectedWorldButton;

        private void Start()
        {
            // CreateTestList();
            
            manager.onLevelSelectedAction = delegate(LevelSavePreview preview)
            {
                playSelectedWorldButton.interactable = true;
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
            Debug.Log("Player world: " + preview.name);
        }
    }
}