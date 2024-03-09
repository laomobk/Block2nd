using System;
using Block2nd.Database;
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

        public GameObject GetSlot(int idx)
        {
            if (idx < 0 || idx >= 9)
            {
                return null;
            }

            return slotsTransform.transform.GetChild(idx).gameObject;
        }

        public void SetSlotMesh(int idx, Mesh mesh, int code)
        {
            slotCodes[idx] = code;
            
            var slotGameObject = GetSlot(idx);
            if (slotGameObject == null)
                return;
            DestroyImmediate(slotGameObject.GetComponent<MeshFilter>().sharedMesh, true);
            slotGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
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

                var shapeMesh = meta.shape.GetShapeMesh(255, 0);
                var mesh = new Mesh();
                mesh.vertices = shapeMesh.positions;
                mesh.uv = shapeMesh.texcoords;
                mesh.triangles = shapeMesh.triangles;
                mesh.RecalculateNormals();
                SetSlotMesh(i, mesh, code);
            }
        }
    }
}