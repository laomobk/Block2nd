using System;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.GUI.GameGUI
{
    public class InventoryUI : MonoBehaviour
    {
        private Transform slotsTransform;
        private RectTransform selectorRectTransform;
        private int[] slotCodes = new[] {0, 0, 0, 0, 0, 0, 0, 0, 0};

        private void Awake()
        {
            selectorRectTransform = transform.Find("Canvas/Selector").GetComponent<RectTransform>();
            slotsTransform = transform.Find("Slots");
        }

        private void Start()
        {
            transform.Find("Canvas/SelectItems").gameObject.SetActive(Application.isMobilePlatform);
            transform.Find("Canvas/_BG").gameObject.SetActive(Application.isMobilePlatform);
        }

        public ItemSlot GetSlot(int idx)
        {
            if (idx < 0 || idx >= 9)
            {
                return null;
            }

            return slotsTransform.transform.GetChild(idx).GetComponent<ItemSlot>();
        }

        public void SetSlotMesh(int idx, BlockMeta meta)
        {
            var shapeMesh = meta.shape.GetGuiShapeMesh(out bool isCube, out int atlasTextureId, out var _);
            slotCodes[idx] = meta.blockCode;
            
            var slot = GetSlot(idx);
            if (slot == null)
                return;
            
            if (isCube)
                slot.SetRenderMode(SlotRenderMode.CUBE);
            else
                slot.SetRenderMode(SlotRenderMode.PLANE);
            
            slot.SetMesh(shapeMesh);
        }

        public void RenderInventory(Inventory inventory)
        {
            var pos = selectorRectTransform.anchoredPosition;
            pos.x = -147 + inventory.selectIdx * 37;
            selectorRectTransform.anchoredPosition = pos;
            
            for (int i = 0; i < 9; ++i)
            {
                var slotCode = slotCodes[i];
                var code = inventory.blockCodes[i];
                
                if (slotCode == code)
                    continue;

                var meta = BlockMetaDatabase.GetBlockMetaByCode(code);
                if (meta == null)
                    continue;

                SetSlotMesh(i, meta);
            }
        }
    }
}