using System;
using Block2nd.Client;
using Block2nd.GameSave;
using Block2nd.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.CreateWorldUI
{
    public class CreateWorldUIManager : MonoBehaviour
    {
        private string willBeSaveName;
        private int terrainType = 0;
        
        public Text willBeSaveText;
        public InputField inputField;
        public Text worldTypeButtonText;

        private void Start()
        {
            UpdateWillBeSaveText();
            worldTypeButtonText.text =
                "World Type: " + BuiltinChunkGeneratorFactory.GetChunkGeneratorNameFromId(terrainType);
        }

        private void UpdateWillBeSaveText()
        {
            var text = inputField.text;

            if (text.Length == 0)
            {
                text = "New World";
            }

            var folderName = LevelSaveManager.GetLevelFolderName(text);
            willBeSaveText.text = "Will be saved in: " + folderName;

            willBeSaveName = folderName;
        }

        public void OnCreateWorld()
        {
            var seed = (int) DateTime.Now.Ticks;
            ClientSharedData.levelSavePreviewInLastContext = new LevelSavePreview
            {
                folderName = willBeSaveName,
                name = inputField.text,
                seed = seed,
                terrainType = terrainType
            };
            
            Debug.Log("Create New World: " + inputField.text + " | " + willBeSaveName + "|" + seed);

            SceneManager.LoadScene("Game");
        }

        public void OnInputFieldChanged()
        {
            UpdateWillBeSaveText();
        }

        public void NextTerrainType()
        {
            terrainType = (terrainType + 1) % 3;
            worldTypeButtonText.text =
                "World Type: " + BuiltinChunkGeneratorFactory.GetChunkGeneratorNameFromId(terrainType);
        }
    }
}