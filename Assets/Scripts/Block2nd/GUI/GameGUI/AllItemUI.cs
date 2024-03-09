using System;
using Block2nd.Database;
using Block2nd.GamePlay;
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
        
        private Player player;
        private InventoryUI inventoryUI;
        
        private Transform slotsTransform;
        private Transform gridRootTransform;
        
        private int beginIndex = 0;

        public readonly int gridCount = 72;

        private void Awake()
        {
            inventoryUI = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryUI>();
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            gridRootTransform = transform.Find("Canvas/Clickables").transform;
            slotsTransform = transform.Find("Slots");
        }

        private void Start()
        {
            FillSlots();
            SetupGridButtons();
        }

        private void SetupGridButtons()
        {
            for (int i = 0; i < gridRootTransform.childCount; ++i)
            {
                gridRootTransform.GetChild(i).GetComponent<Button>().onClick.AddListener(
                    new AllItemGridClickEvent(i, this).Invoke);
            }
        }
        
        public void SetSlotMesh(int idx, Mesh mesh)
        {
            idx -= beginIndex;
            
            var slotGameObject = slotsTransform.GetChild(idx).gameObject;
            
            if (slotGameObject == null)
                return;
            
            DestroyImmediate(slotGameObject.GetComponent<MeshFilter>().sharedMesh, true);
            slotGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        private void FillSlots()
        {
            for (int i = beginIndex, j = 0; i < BlockMetaDatabase.blocks.Count && j < 72; ++i, ++j)
            {
                var meta = BlockMetaDatabase.GetBlockMetaByCode(i + 1);
                if (meta == null)
                    continue;

                var shapeMesh = meta.shape.GetShapeMesh(255, 0);
                var mesh = new Mesh();
                mesh.vertices = shapeMesh.positions;
                mesh.uv = shapeMesh.texcoords;
                mesh.triangles = shapeMesh.triangles;
                mesh.RecalculateNormals();
                SetSlotMesh(i, mesh);
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