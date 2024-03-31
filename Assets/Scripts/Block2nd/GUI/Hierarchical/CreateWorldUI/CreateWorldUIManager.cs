using System;
using Block2nd.Client;
using Block2nd.GameSave;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.CreateWorldUI
{
    public class CreateWorldUIManager : MonoBehaviour
    {
        private string willBeSaveName;
        
        public Text willBeSaveText;
        public InputField inputField;

        private void Start()
        {
            UpdateWillBeSaveText();
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
                seed = seed
            };
            
            Debug.Log("Create New World: " + inputField.text + " | " + willBeSaveName + "|" + seed);

            SceneManager.LoadScene("Game");
        }

        public void OnInputFieldChanged()
        {
            UpdateWillBeSaveText();
        }
    }
}