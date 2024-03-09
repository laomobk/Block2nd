using System;
using Block2nd.GamePlay;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.GameGUI
{
    
    public class InventoryGridManager : MonoBehaviour
    {
        class InventoryGridClickEvent
        {
            private int idx;
            private InventoryGridManager manager;
        
            public InventoryGridClickEvent(int idx, InventoryGridManager manager)
            {
                this.idx = idx;
                this.manager = manager;
            }

            public void Invoke()
            {
                manager.OnGridClick(idx);
            }
        }
        
        private void Start()
        {
            SetupGridButtons();
        }

        private void SetupGridButtons()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).GetComponent<Button>().onClick.AddListener(
                    new InventoryGridClickEvent(i, this).Invoke);
            }
        }

        private void OnGridClick(int idx)
        {
            var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            player.Inventory.selectIdx = idx;
            player.UpdateHoldingItem();
        }
    }
}