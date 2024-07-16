using System.Collections.Generic;
using Block2nd.Database;
using UnityEngine;
using UnityEngine.Profiling;

namespace Block2nd.World
{
    public class ChunkRenderEntity : MonoBehaviour
    {
        private GameObject subTransparentChunk;
        private GameObject subLiquidChunk;
        private GameObject subPlantChunk;

        private bool visible = false;
        
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
        
        private List<Vector3> ptVert = new List<Vector3>();
        private List<Vector2> ptUvs = new List<Vector2>();
        private List<Color> ptColors = new List<Color>();
        private List<int> ptTris = new List<int>();

        [HideInInspector] public ulong currentCoordKey;
        [HideInInspector] public int freeCount = 0;
        [HideInInspector] public bool brandNew = true;
        
        private void Awake()
        {
            subTransparentChunk = transform.GetChild(0).gameObject;
            subLiquidChunk = transform.GetChild(1).gameObject;
            subPlantChunk = transform.GetChild(2).gameObject;
        }

        private void OnDestroy()
        {
            DestroyImmediate(GetComponent<MeshFilter>().sharedMesh, true);
            DestroyImmediate(subTransparentChunk.GetComponent<MeshFilter>().sharedMesh, true);
            DestroyImmediate(subLiquidChunk.GetComponent<MeshFilter>().sharedMesh, true);
        }

        public void SetVisible(bool state)
        {
            if (state != visible)
                visible = state;
            else
                return;
            
            GetComponent<MeshRenderer>().enabled = state;
            subTransparentChunk.GetComponent<MeshRenderer>().enabled = state;
            subLiquidChunk.GetComponent<MeshRenderer>().enabled = state;
            subPlantChunk.GetComponent<MeshRenderer>().enabled = state;
        }

        public void RenderChunk(Chunk chunk, bool force = false)
        {
            if (force)
            {
                SetVisible(true);
            }
            else
            {
                if (!chunk.dirty && chunk.CoordKey == currentCoordKey && !brandNew)
                {
                    SetVisible(true);
                    return;
                }
            }

            brandNew = false;
            currentCoordKey = chunk.CoordKey;

            Profiler.BeginSample("Render Chunk Mesh");

            if (chunk.lightingState < 1)
            {
                // TODO: consider it.
                chunk.BakeHeightMapWithSkyLightUpdate();
                chunk.lightingState = 1;
            }
            else
            {
                chunk.BakeHeightMap();
            }
            
            if (chunk.lightingState < 2)
            {
                chunk.UpdateChunkLightMapFullyNew();
            } else if (chunk.lightingState < 3)
            {
                chunk.CheckAndUpdateSkyLights();
            }

            var chunkBlocks = chunk.chunkBlocks;
            
            var opMesh = new Mesh();
            var trMesh = new Mesh();
            var lqMesh = new Mesh();
            var ptMesh = new Mesh();
            
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
            ptTris.Clear();
            ptColors.Clear();
            ptUvs.Clear();
            ptVert.Clear();

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
                            int exposedFace = meta.forceRenderAllFace
                                ? 255
                                : (meta.transparent
                                    ? chunk.GetExposedFaceTransparent(x, y, z, meta.blockCode)
                                    : chunk.GetExposedFace(x, y, z));
                            
                            if (exposedFace == 0)
                                continue;
                            
                            if (meta.liquid)
                            {
                                tris = lqTris;
                                uvs = lqUvs;
                                colors = lqColors;
                                vert = lqVert;
                            } else if (meta.plant)
                            {
                                tris = ptTris;
                                uvs = ptUvs;
                                colors = ptColors;
                                vert = ptVert;
                            }
                            else if (meta.transparent)
                            {
                                tris = trTris;
                                uvs = trUvs;
                                colors = trColors;
                                vert = trVert;
                            }
                            
                            var blockMesh = behavior.OnRender(chunk.LocalToWorld(x, y, z));
                            if (blockMesh == null)
                                blockMesh = meta.shape.GetShapeMesh(
                                    exposedFace,
                                    chunk.GetLightAttenuation(x, y, z, exposedFace));
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

            lqMesh.vertices = lqVert.ToArray();
            lqMesh.uv = lqUvs.ToArray();
            lqMesh.SetColors(lqColors);
            lqMesh.triangles = lqTris.ToArray();
            lqMesh.RecalculateNormals();

            ptMesh.vertices = ptVert.ToArray();
            ptMesh.uv = ptUvs.ToArray();
            ptMesh.SetColors(ptColors);
            ptMesh.triangles = ptTris.ToArray();
            ptMesh.RecalculateNormals();

            var curTrMesh = subTransparentChunk.GetComponent<MeshFilter>().sharedMesh;
            var curLqMesh = subLiquidChunk.GetComponent<MeshFilter>().sharedMesh;
            var curPtMesh = subLiquidChunk.GetComponent<MeshFilter>().sharedMesh;
            var curOpMesh = GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(curOpMesh, true);
            DestroyImmediate(curTrMesh, true);
            DestroyImmediate(curLqMesh, true);
            DestroyImmediate(curPtMesh, true);
            
            GetComponent<MeshFilter>().sharedMesh = opMesh;
            subTransparentChunk.GetComponent<MeshFilter>().sharedMesh = trMesh;
            subLiquidChunk.GetComponent<MeshFilter>().sharedMesh = lqMesh;
            subPlantChunk.GetComponent<MeshFilter>().sharedMesh = ptMesh;

            chunk.dirty = false;
            chunk.firstRendered = true;
            brandNew = false;
            
            SetVisible(true);

            Profiler.EndSample();
        }
        

        public byte GetAoBits(Chunk chunk, int x, int y, int z, int exposedFace)
        {
            // cell: 32 位数字, 每 4 bit 为一个顶点的遮蔽数值，共 8 个顶点.
            // 从低到高分别代表顶点 B(LT, RT, LB, RB), F(LT, RT, LB, RB)
            
            byte vertAoValue = 0;
            byte exposedVertices = 0;
            
            if ((exposedFace & 1) != 0)
            {
                exposedVertices |= 240;
            }

            if ((exposedFace & 2) != 0)
            {
                exposedVertices |= 15;
            }

            if ((exposedFace & 4) != 0)
            {
                exposedVertices |= 85;
            }

            if ((exposedFace & 8) != 0)
            {
                exposedVertices |= 170;
            }

            if ((exposedFace & 16) != 0)
            {
                exposedVertices |= 51;
            }

            if ((exposedFace & 32) != 0)
            {
                exposedVertices |= 204;
            }

            if ((exposedVertices & 1) != 0)
            {
                if (!(chunk.GetBlock(x + 1, y + 1, z - 1, true, true).Transparent() &&
                      chunk.GetBlock(x + 1, y + 1, z, true, true).Transparent() &&
                      chunk.GetBlock(x, y + 1, z - 1, true, true).Transparent()))
                {
                    vertAoValue |= 1;
                }
            }
            
            if ((exposedVertices & 2) != 0)
            {
                if (!(chunk.GetBlock(x - 1, y + 1, z - 1, true, true).Transparent() &&
                      chunk.GetBlock(x - 1, y + 1, z, true, true).Transparent() &&
                      chunk.GetBlock(x, y + 1, z - 1, true, true).Transparent()))
                {
                    vertAoValue |= 2;
                }
            }
            
            if ((exposedVertices & 4) != 0)
            {
                if (!(chunk.GetBlock(x + 1, y - 1, z - 1, true, true).Transparent() &&
                      chunk.GetBlock(x + 1, y - 1, z, true, true).Transparent() &&
                      chunk.GetBlock(x, y - 1, z - 1, true, true).Transparent()))
                {
                    vertAoValue |= 4;
                }
            }
            
            if ((exposedVertices & 8) != 0)
            {
                if (!(chunk.GetBlock(x - 1, y + 1, z - 1, true, true).Transparent() &&
                      chunk.GetBlock(x - 1, y + 1, z, true, true).Transparent() &&
                      chunk.GetBlock(x, y - 1, z - 1, true, true).Transparent()))
                {
                    vertAoValue |= 8;
                }
            }
            
            if ((exposedVertices & 16) != 0)
            {
                if (!(chunk.GetBlock(x + 1, y + 1, z + 1, true, true).Transparent() &&
                      chunk.GetBlock(x + 1, y + 1, z, true, true).Transparent() &&
                      chunk.GetBlock(x, y + 1, z + 1, true, true).Transparent()))
                {
                    vertAoValue |= 16;
                }
            }
            
            if ((exposedVertices & 32) != 0)
            {
                if (!(chunk.GetBlock(x - 1, y + 1, z + 1, true, true).Transparent() &&
                      chunk.GetBlock(x - 1, y + 1, z, true, true).Transparent() &&
                      chunk.GetBlock(x, y + 1, z + 1, true, true).Transparent()))
                {
                    vertAoValue |= 32;
                }
            }
            
            if ((exposedVertices & 64) != 0)
            {
                if (!(chunk.GetBlock(x + 1, y - 1, z + 1, true, true).Transparent() &&
                      chunk.GetBlock(x + 1, y - 1, z, true, true).Transparent() &&
                      chunk.GetBlock(x, y - 1, z + 1, true, true).Transparent()))
                {
                    vertAoValue |= 64;
                }
            }
            
            if ((exposedVertices & 128) != 0)
            {
                if (!(chunk.GetBlock(x - 1, y - 1, z + 1, true, true).Transparent() &&
                      chunk.GetBlock(x - 1, y - 1, z, true, true).Transparent() &&
                      chunk.GetBlock(x, y - 1, z + 1, true, true).Transparent()))
                {
                    vertAoValue |= 128;
                }
            }

            return vertAoValue;
        }
    }
}