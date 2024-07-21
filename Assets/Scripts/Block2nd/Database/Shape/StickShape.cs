using Block2nd.Utils;
using UnityEngine;

namespace Block2nd.Database.Meta
{
    public class StickShape : BlockShape
    {
        protected static Vector3[] positions;
        protected static int[] triangles;
        protected static Vector2[] texcoords;
        protected static Color[] colors;

        protected Vector2 originUV;
        protected Color color;
        protected int uvIdx;

        protected BlockMesh guiBlockMesh = new BlockMesh();
        
        public StickShape(int uvIdx, Color color)
        {
            originUV = AtlasTextureDescriptor.Default.GetUVByIndex(uvIdx) + new Vector2(0.02734375f, 0);
            this.color = color;
            this.uvIdx = uvIdx;
            
            InitStickMeshData();
        }

        protected void InitStickMeshData()
        {
            var meshBuilder = new MeshBuilder();

            meshBuilder.SetQuadColor(color, color, color, color);
            meshBuilder.SetQuadUV(originUV, 0.0078125f, 0.0390625f);
            
            // Front
            meshBuilder.AddQuad(
                new Vector3(0.525f, 0, 0.525f), Vector3.left, Vector3.up, 0.125f, 0.625f, true);
            
            // Back
            meshBuilder.AddQuad(
                new Vector3(0.4f, 0, 0.4f), Vector3.right, Vector3.up, 0.125f, 0.625f, true);
            
            // Left
            meshBuilder.AddQuad(
                new Vector3(0.525f, 0, 0.4f), Vector3.forward, Vector3.up, 0.125f, 0.625f, true);
            
            // Right
            meshBuilder.AddQuad(
                new Vector3(0.4f, 0, 0.525f), Vector3.back, Vector3.up, 0.125f, 0.625f, true);
            
            // Top
            meshBuilder.SetQuadUV(originUV + new Vector2(0, 0.03125f), 0.0078125f, 0.0078125f);
            meshBuilder.AddQuad(
                new Vector3(0.525f, 0.625f, 0.525f), Vector3.left, Vector3.back, 0.125f, 0.125f, true);
            
            // Buttom
            meshBuilder.SetQuadUV(originUV, 0.0078125f, 0.0078125f);
            meshBuilder.AddQuad(
                new Vector3(0.525f, 0, 0.4f), Vector3.left, Vector3.forward, 0.125f, 0.125f, true);

            positions = meshBuilder.vertices.ToArray();
            triangles = meshBuilder.indices.ToArray();
            texcoords = meshBuilder.texcoords.ToArray();
            colors = meshBuilder.colors.ToArray();
            
            var guiMeshBuilder = new MeshBuilder();
            guiMeshBuilder.SetQuadUV(
                AtlasTextureDescriptor.Default.GetUVByIndex(uvIdx), 0.0625f, 0.0625f);
            guiMeshBuilder.AddQuad(Vector3.zero, Vector3.right, Vector3.up, 1, 1);
            guiBlockMesh.positions = guiMeshBuilder.vertices.ToArray();
            guiBlockMesh.texcoords = guiMeshBuilder.texcoords.ToArray();
            guiBlockMesh.triangles = guiMeshBuilder.indices.ToArray();
        }
        
        public override BlockMesh GetShapeMesh(int exposedFace, long lightAttenuation, int aoBits = 0)
        {
            return new BlockMesh
            {
                positions = positions,
                triangles = triangles,
                texcoords = texcoords,
                colors = colors,
                
                positionCount = 24,
                triangleCount = 36,
                texcoordCount = 24,
                colorsCount = 24,
            };
        }

        public override BlockMesh GetGuiShapeMesh(out bool isCube, out int atlasTextureId, out int uvIdx)
        {
            isCube = false;
            atlasTextureId = 0;
            uvIdx = this.uvIdx;
            return guiBlockMesh;
        }
    }
}