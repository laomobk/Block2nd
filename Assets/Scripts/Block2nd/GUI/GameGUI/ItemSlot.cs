using System;
using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.GUI.GameGUI
{
    public enum SlotRenderMode
    {
        PLANE = 0,
        CUBE = 1,
    }

    public class ItemSlot : MonoBehaviour
    {
        private Mesh currentMesh;
        private Material currentMaterial;
        
        private Transform slot;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private static TransformSettings _cubeTransformSettings = new TransformSettings
        {
            localPos = Vector3.zero,
            localEulerAngels = new Vector3(-24.258f, 52.057f, -28.144f),
            localScale = new Vector3(2, 2, 2)
        };

        private static TransformSettings _planeTransformSettings = new TransformSettings
        {
            localPos = new Vector3(-0.25f, -0.73f, 0),
            localEulerAngels = new Vector3(0, 0, 0),
            localScale = new Vector3(3.27f, 3.29f, 2f)
        };

        private SlotRenderMode currentRenderMode = SlotRenderMode.CUBE;

        public void SetRenderMode(SlotRenderMode mode)
        {
            if (currentRenderMode == mode) 
                return;

            currentRenderMode = mode;
            ApplyMode();
        }

        private void Awake()
        {
            currentMesh = new Mesh();
            currentMaterial = new Material(Shader.Find("B2nd/Block2nd_GUICube"));
            
            slot = transform.GetChild(0);
            meshFilter = slot.GetComponent<MeshFilter>();
            meshRenderer = slot.GetComponent<MeshRenderer>();
            
            meshFilter.sharedMesh = currentMesh;
            meshRenderer.sharedMaterial = currentMaterial;
        }

        private void ApplyMode()
        {
            currentMaterial.SetInt("_GuiItemMode", (int) currentRenderMode);
            
            switch (currentRenderMode)
            {
                case SlotRenderMode.CUBE:
                    _cubeTransformSettings.Apply(slot.transform);
                    return;
                case SlotRenderMode.PLANE:
                    _planeTransformSettings.Apply(slot.transform);
                    return;
            }
        }

        private void Start()
        {
            ApplyMode();
        }

        public void SetMesh(BlockMesh blockMesh)
        {
            currentMesh.Clear();
            currentMesh.vertices = blockMesh.positions;
            currentMesh.triangles = blockMesh.triangles;
            currentMesh.uv = blockMesh.texcoords;
        }
    }
}