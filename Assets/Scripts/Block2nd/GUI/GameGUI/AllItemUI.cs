using System;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.GamePlay;
using Block2nd.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.GameGUI
{
    
    
    public class AllItemUI : MonoBehaviour, IGameGUI
    {
        class AllItemGridClickEvent
        {
            private int idx;
            private AllItemUI ui;
        
            public AllItemGridClickEvent(int idx, AllItemUI ui)
            {
                this.idx = idx;
                this.ui = ui;
            }

            public void Invoke()
            {
                ui.OnGridClick(idx);
            }
        }

        [SerializeField] private GameObject itemSlotPrefab;

        private Player player;
        private InventoryUI inventoryUI;

        private Transform slotsTransform;
        private Transform gridRootTransform;
        
        private int beginIndex = 0;

        public readonly int gridCount = 72;

        private void Awake()
        {
            inventoryUI = GameObject.FindGameObjectWithTag("Inventory")?.GetComponent<InventoryUI>();
            player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
            gridRootTransform = transform.Find("Canvas/Clickables").transform;
            slotsTransform = transform.Find("Slots");
        }

        private void Start()
        {
            InitItemSlots();
            SetupGridButtons();
            FillSlots();
        }

        private void SetupGridButtons()
        {
            for (int i = 0; i < gridRootTransform.childCount; ++i)
            {
                gridRootTransform.GetChild(i).GetComponent<Button>().onClick.AddListener(
                    new AllItemGridClickEvent(i, this).Invoke);
            }
        }

        private void InitItemSlots()
        {
            var layer = LayerMask.NameToLayer("GUIPanel");
            float beginX = -2.51f, beginY = 0;
            
            for (int i = 0; i < 9; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    var slot = Instantiate(itemSlotPrefab, slotsTransform);
                    GameObjectUtils.SetLayerToAllChildrenAndSelf(slot.transform, layer);
                    
                    var slotTransform = slot.transform;
                    slotTransform.localPosition = new Vector3(beginX + j * 3.52f, beginY - i * 3.52f);
                    slotTransform.localScale = new Vector3(0.84757f, 0.84757f, 0.84757f);
                }
            }
        }

        public void SetSlotMesh(int idx, BlockMeta meta)
        {
            idx -= beginIndex;
            
            var shapeMesh = meta.shape.GetGuiShapeMesh(out bool isCube, out int atlasTextureId, out var _);
            
            var slot = slotsTransform.GetChild(idx).GetComponent<ItemSlot>();
            if (slot == null)
                return;
            
            if (isCube)
                slot.SetRenderMode(SlotRenderMode.CUBE);
            else
                slot.SetRenderMode(SlotRenderMode.PLANE);
            
            slot.SetMesh(shapeMesh);
        }

        private void FillSlots()
        {
            for (int i = beginIndex, j = 0; i < BlockMetaDatabase.blocks.Count && j < 72; ++i, ++j)
            {
                var meta = BlockMetaDatabase.GetBlockMetaByCode(i + 1);
                if (meta == null)
                    continue;
                
                SetSlotMesh(i, meta);
            }
        }

        public void PushToInventory(int blockCode)
        {
            if (blockCode >= BlockMetaDatabase.blocks.Count)
                return;

            var codes = player.Inventory.blockCodes;
            
            for (int i = codes.Length - 1; i > 0; --i)
            {
                codes[i] = codes[i - 1];
            }

            codes[0] = blockCode;
            
            player.UpdateHoldingItem();
        }

        private void OnGridClick(int idx) {
            PushToInventory(beginIndex + idx + 1);
        }

        public void OnCloseGUI()
        {
            gameObject.SetActive(false);
        }

        public void OnOpenGUI()
        {
            gameObject.SetActive(true);
        }
    }
}