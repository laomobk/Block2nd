using System;
using Block2nd.Client;
using Block2nd.GameSave;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical.DeleteWorldUI
{
    public class DeleteWorldPage : MonoBehaviour
    {
        public Text warnText;

        private LevelSavePreview preview;

        public void Start()
        {
            preview = ClientSharedData.levelSavePreviewInLastContext;
            warnText.text = "'" + preview.name + "' will be lost forever! (A long time!)";
        }

        public void OnDeleteButtonClick()
        {
            LevelSaveManager.DeleteSave(preview);
        }
    }
}