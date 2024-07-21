using System;
using Block2nd.Database;
using Block2nd.Utils;
using UnityEngine;

namespace TestVoxelizer
{
    public class Tester : MonoBehaviour
    {
        public Texture2D texture;

        private MeshFilter meshFilter;
        private Mesh mesh;

        private void Awake()
        {
            mesh = new Mesh();
            meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
        }

        public void Run()
        {
            var task = new VoxelizeTask(texture, AtlasTextureDescriptor.Default.GetUVByIndex(80), 1 / 16f, 1 / 16f);

            var blockMesh = PixelVoxelizer.Do(task);

            mesh.Clear();
            mesh.vertices = blockMesh.positions;
            mesh.uv = blockMesh.texcoords;
            mesh.triangles = blockMesh.triangles;
            mesh.RecalculateNormals();
        }
    }
}