using System;
using Block2nd.Database;
using Block2nd.GUI.GameGUI;
using Block2nd.Utils;
using UnityEngine;

namespace TestUI
{
    public class TestCube : MonoBehaviour
    {
        public ItemSlot slot;
        
        private MeshFilter meshFilter;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        private void Start()
        {
            var shapeMesh = BlockMetaDatabase.GetBlockMetaById("b2nd:block/torch").shape
                .GetGuiShapeMesh(out var isCube, out var _, out var _);
            
            slot.SetRenderMode(isCube ? SlotRenderMode.CUBE : SlotRenderMode.PLANE);
            slot.SetMesh(shapeMesh);
        }
    }
}