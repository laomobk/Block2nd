using System.Collections.Generic;
using Block2nd.Database;
using UnityEngine;

namespace Block2nd.World
{
    public class ChunkRenderEntity : MonoBehaviour
    {
        private GameObject subTransparentChunk;
        private GameObject subLiquidChunk;
        
        private List<Vector3> opVert = new List<Vector3>();
        private List<Vector2> opUvs = new List<Vector2>();
        private List<Color> opColors = new List<Color>();
        private List<int> opTris = new List<int>();
            
        private List<Vector3> trVert = new List<Vector3>();
        private List<Vector2> trUvs = new List<Vector2>();
        private List<Color> trColors = new List<Color>();
        private List<int> trTris = new List<int>();
        
        private List<Vector3> lqVert = new List<Vector3>();
        private List<Vector2> lqUvs = new List<Vector2>();
        private List<Color> lqColors = new List<Color>();
        private List<int> lqTris = new List<int>();
        
        private void Awake()
        {
            subTransparentChunk = transform.GetChild(0).gameObject;
            subLiquidChunk = transform.GetChild(1).gameObject;
        }

        private void OnDestroy()
        {
            DestroyImmediate(GetComponent<MeshFilter>().sharedMesh, true);
            DestroyImmediate(subTransparentChunk.GetComponent<MeshFilter>().sharedMesh, true);
            DestroyImmediate(subLiquidChunk.GetComponent<MeshFilter>().sharedMesh, true);
        }
        
        public void RenderChunk(Chunk chunk)
        {
            var chunkBlocks = chunk.chunkBlocks;
            
            var opMesh = new Mesh();
            var trMesh = new Mesh();
            var lqMesh = new Mesh();
            
            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);
            
            trColors.Clear();
            trTris.Clear();
            trUvs.Clear();
            trVert.Clear();
            opColors.Clear();
            opTris.Clear();
            opUvs.Clear();
            opVert.Clear();
            lqColors.Clear();
            lqTris.Clear();
            lqUvs.Clear();
            lqVert.Clear();
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < width; z++)
                    {
                        var block = chunk.GetBlock(x, y, z);
                        int code = block.blockCode;
                        var meta = BlockMetaDatabase.GetBlockMetaByCode(code);
                        var behavior = BlockMetaDatabase.GetBlockBehaviorByCode(code);

                        var tris = opTris;
                        var uvs = opUvs;
                        var colors = opColors;
                        var vert = opVert;
                        
                        if (meta != null)
                        {
                            if (meta.liquid)
                            {
                                tris = lqTris;
                                uvs = lqUvs;
                                colors = lqColors;
                                vert = lqVert;
                            } else if (meta.transparent)
                            {
                                tris = trTris;
                                uvs = trUvs;
                                colors = trColors;
                                vert = trVert;
                            }
                            
                            var blockMesh = behavior.OnRender(chunk.LocalToWorld(x, y, z));
                            if (blockMesh == null)
                                blockMesh = meta.shape.GetShapeMesh(
                                    meta.forceRenderAllFace ? 255 : (meta.transparent ? 
                                        chunk.GetExposedFaceTransparent(x, y, z, meta.blockCode) :
                                        chunk.GetExposedFace(x, y, z)),
                                    chunk.GetLightAttenuation(x, y, z));
                            var triangleStartIdx = vert.Count;

                            for (int i = 0; i < blockMesh.triangleCount; ++i)
                            {
                                tris.Add(triangleStartIdx + blockMesh.triangles[i]);
                            }

                            for (int i = 0; i < blockMesh.positionCount; ++i)
                            {
                                vert.Add(blockMesh.positions[i] + new Vector3(x, y, z));
                            }

                            for (int i = 0; i < blockMesh.texcoordCount; ++i)
                            {
                                uvs.Add(blockMesh.texcoords[i]);
                            }

                            for (int i = 0; i < blockMesh.colorsCount; ++i)
                            {
                                colors.Add(blockMesh.colors[i]);
                            }
                        }
                    }
                }
            }

            opMesh.vertices = opVert.ToArray();
            opMesh.uv = opUvs.ToArray();
            opMesh.SetColors(opColors);
            opMesh.triangles = opTris.ToArray();
            opMesh.RecalculateNormals();
            opMesh.RecalculateBounds();
            
            trMesh.vertices = trVert.ToArray();
            trMesh.uv = trUvs.ToArray();
            trMesh.SetColors(trColors);
            trMesh.triangles = trTris.ToArray();
            trMesh.RecalculateNormals();
            trMesh.RecalculateBounds();

            lqMesh.vertices = lqVert.ToArray();
            lqMesh.uv = lqUvs.ToArray();
            lqMesh.SetColors(lqColors);
            lqMesh.triangles = lqTris.ToArray();
            lqMesh.RecalculateNormals();
            lqMesh.RecalculateBounds();

            var curTrMesh = subTransparentChunk.GetComponent<MeshFilter>().sharedMesh;
            var curLqMesh = subLiquidChunk.GetComponent<MeshFilter>().sharedMesh;
            var curOpMesh = GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(curOpMesh, true);
            DestroyImmediate(curTrMesh, true);
            DestroyImmediate(curLqMesh, true);
            
            GetComponent<MeshFilter>().sharedMesh = opMesh;

            subTransparentChunk.GetComponent<MeshFilter>().sharedMesh = trMesh;

            subLiquidChunk.GetComponent<MeshFilter>().sharedMesh = lqMesh;
        }
    }
}