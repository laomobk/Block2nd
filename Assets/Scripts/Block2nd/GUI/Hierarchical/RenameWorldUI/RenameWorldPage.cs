using System;
using Block2nd.Client;
using Block2nd.GameSave;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.RenameWorldUI
{
    public class RenameWorldPage : MonoBehaviour
    {
        public InputField inputField;

        private LevelSavePreview preview;

        private void Start()
        {
            preview = ClientSharedData.levelSavePreviewInLastContext;
            inputField.text = preview.name;
        }

        public void OnRenameButtonClick()
        {
            LevelSaveManager.RenameSave(preview, inputField.text);
        }
    }
}