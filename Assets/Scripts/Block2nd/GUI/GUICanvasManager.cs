using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.GUI.GameGUI;
using Block2nd.GUI.Hierarchical;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class GUICanvasManager : MonoBehaviour
    {
        public HierarchicalMenu gameMenu;
        public Text holdingItemNameText;
        public Text gameVersionText;
        public Text updateText;
        public WorldGeneratingProgressUI worldGeneratingProgressUI;
        public GameObject mobileUICanvas;
        public InventoryUI inventoryUI;
        public AllItemUI allItemUI;
        public ChatUI chatUI;
        public ChunkStatText chunkStatText;

        private float holdingItemNameTextAlpha;

        public void SetGUIItemNameText(string textToDisplay)
        {
            holdingItemNameText.text = textToDisplay;
            holdingItemNameTextAlpha = 2;
        }

        public void SetGameVersionText(string versionString)
        {
            gameVersionText.text = versionString;
        }

        private void UpdateHoldingItemNameText()
        {
            if (holdingItemNameTextAlpha > 0)
            {
                holdingItemNameTextAlpha -= Time.deltaTime * 2;
            }

            var color = holdingItemNameText.color;
            color.a = holdingItemNameTextAlpha;
            holdingItemNameText.color = color;
        }

        public void SetUpdateText(int nUpdate)
        {
            // updateText.text = nUpdate + " update event" + ((nUpdate > 1) ? "s" : "") + ".";
        }

        private void Update()
        {
            UpdateHoldingItemNameText();
        }

        public void SetGUIBackgroundState(bool state)
        {
            gameMenu.SetBackgroundState(state);
        }
    }
}
