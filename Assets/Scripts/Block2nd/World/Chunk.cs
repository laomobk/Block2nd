using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Block2nd.Behavior;
using Block2nd.Behavior.Block;
using Block2nd.Client;
using Block2nd.Database;
using Block2nd.MathUtil;
using Block2nd.UnsafeStructure;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Block2nd.World
{
    public class Chunk : MonoBehaviour
    {
        public IntVector3 worldBasePosition;
        public Bounds aabb;
        
        public bool rendered = false;
        
        public ChunkBlockData[,,] chunkBlocks;
        public int[,] heightMap;
        public byte[,,] ambientOcclusionMap;

        private ChunkManager locatedChunkManager;

        private GameClient gameClient;
        private GameObject subTransparentChunk;
        private GameObject subLiquidChunk;
        private MeshCollider meshCollider;

        private bool instanced = false;

        public bool dirty = false;
        
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

        // private FastBuffer<Vector3> opVert = new FastBuffer<Vector3>();
        // private FastBuffer<Vector2> opUvs = new FastBuffer<Vector2>();
        // private FastBuffer<Color> opColors = new FastBuffer<Color>();
        // private FastBuffer<int> opTris = new FastBuffer<int>();
        
        // private FastBuffer<Vector3> trVert = new FastBuffer<Vector3>();
        // private FastBuffer<Vector2> trUvs = new FastBuffer<Vector2>();
        // private FastBuffer<Color> trColors = new FastBuffer<Color>();
        // private FastBuffer<int> trTris = new FastBuffer<int>();

        private void Awake()
        {
            subTransparentChunk = transform.GetChild(0).gameObject;
            subLiquidChunk = transform.GetChild(1).gameObject;
            meshCollider = GetComponent<MeshCollider>();
        }

        private void Start()
        {
            gameClient = FindObjectOfType<GameClient>();
        }

        private void OnDestroy()
        {
            DestroyImmediate(GetComponent<MeshFilter>().sharedMesh, true);
            DestroyImmediate(subTransparentChunk.GetComponent<MeshFilter>().sharedMesh, true);
        }

        private float CalculateLightAttenuation(int x, int y, int z)
        {
            for (; y < 10; ++y)
            {
                var code = GetBlock(x, y + 1, z).blockCode;
                if (code != 0)
                {
                    return 0.8f;
                }
            }

            return 1f;
        }

        private void ReplaceOrAdd<T>(ref List<T> list, T val, ref bool overflowed, ref int count)
        {
            if (rendered && !overflowed)
            {
                list[count++] = val;
                if (count >= list.Count)
                {
                    overflowed = true;
                }
            }
            else
            {
                list.Add(val);
            }
        }

        private void InstantiateAllBlocks()
        {
            
            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < width; z++)
                    {
                        chunkBlocks[x, y, z].behaviorInstance = BlockMetaDatabase.GetBlockBehaviorByCode(
                            chunkBlocks[x, y, z].blockCode).CreateInstance();
                    }
                }
            }

            instanced = true;
        }

        public void BakeHeightMap()
        {
            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);
            
            for (int x = 0; x < width; ++x)
            {
                for (int z = 0; z < width; ++z)
                {
                    for (int y = height - 1; y >= 0; --y)
                    {
                        if (chunkBlocks[x, y, z].blockCode != 0)
                        {
                            heightMap[x, z] = y;
                            break;
                        }
                    }
                }
            }
        }

        public void BakeAmbientOcclusionMap()
        {
            // cell: 32 位数字, 每 4 bit 为一个顶点的遮蔽数值，共 8 个顶点.
            // 从低到高分别代表顶点 B(LT, RT, LB, RB), F(LT, RT, LB, RB)
            // 只有不透明方块才会被加入到环境光遮蔽的计算之中
            
            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);
            
            for (int x = 0; x < width; ++x)
            {
                for (int z = 0; z < width; ++z)
                {
                    for (int y = height - 1; y >= 0; --y)
                    {
                        int vertAoValue = 0;

                        if (!GetBlock(x + 1, y + 1, z - 1).Transparent())
                            vertAoValue |= 1;
                        
                        if (!GetBlock(x + 1, y + 1, z).Transparent())
                            vertAoValue |= 21;
                        
                        if (!GetBlock(x + 1, y + 1, z + 1).Transparent())
                            vertAoValue |= 20;

                        if (!GetBlock(x, y + 1, z - 1).Transparent())
                            vertAoValue |= 3;
                        
                        if (!GetBlock(x, y + 1, z + 1).Transparent())
                            vertAoValue |= 60;

                        if (!GetBlock(x - 1, y + 1, z - 1).Transparent())
                            vertAoValue |= 2;
                        
                        if (!GetBlock(x - 1, y + 1, z).Transparent())
                            vertAoValue |= 42;
                        
                        if (!GetBlock(x - 1, y + 1, z + 1).Transparent())
                            vertAoValue |= 40;
                        
                        
                        if (!GetBlock(x + 1, y - 1, z - 1).Transparent())
                            vertAoValue |= 4;
                        
                        if (!GetBlock(x + 1, y - 1, z).Transparent())
                            vertAoValue |= 104;
                        
                        if (!GetBlock(x + 1, y - 1, z + 1).Transparent())
                            vertAoValue |= 100;

                        if (!GetBlock(x, y - 1, z - 1).Transparent())
                            vertAoValue |= 14;
                        
                        if (!GetBlock(x, y - 1, z + 1).Transparent())
                            vertAoValue |= 300;

                        if (!GetBlock(x - 1, y - 1, z - 1).Transparent())
                            vertAoValue |= 10;
                        
                        if (!GetBlock(x - 1, y - 1, z).Transparent())
                            vertAoValue |= 210;
                        
                        if (!GetBlock(x - 1, y - 1, z + 1).Transparent())
                            vertAoValue |= 200;
                    }
                }
            }
        }
        
        public void UpdateChuckMesh()
        {
            if (!instanced)
                InstantiateAllBlocks();
            
            BakeHeightMap();
            
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
                        var block = GetBlock(x, y, z);
                        int code = block.blockCode;
                        var meta = BlockMetaDatabase.GetBlockMetaByCode(code);
                        var behavior = block.behaviorInstance;

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
                            
                            var blockMesh = behavior.OnRender();
                            if (blockMesh == null)
                                blockMesh = meta.shape.GetShapeMesh(
                                    meta.forceRenderAllFace ? 255 : (meta.transparent ? 
                                        GetExposedFaceTransparent(x, y, z, meta.blockCode) :
                                        GetExposedFace(x, y, z)),
                                    GetLightAttenuation(x, y, z));
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
            GetComponent<MeshCollider>().sharedMesh = opMesh;

            subTransparentChunk.GetComponent<MeshFilter>().sharedMesh = trMesh;
            subTransparentChunk.GetComponent<MeshCollider>().sharedMesh = trMesh;

            subLiquidChunk.GetComponent<MeshFilter>().sharedMesh = lqMesh;

            rendered = true;
            dirty = false;
        }

        public void SetChunkBlockData(ChunkBlockData[,,] data)
        {
            chunkBlocks = data;
        }

        public void SetLocatedLevel(ChunkManager chunkManager)
        {
            locatedChunkManager = chunkManager;
        }

        public ChunkBlockData GetBlockWorldPos(int x, int y, int z, bool searchHoldLevel)
        {
            return GetBlock(
                x - worldBasePosition.x,
                y - worldBasePosition.y,
                z - worldBasePosition.z,
                searchHoldLevel);
        }

        public void UpdateBlock(int x, int y, int z)
        {
            var v = new IntVector3(x, y, z);
            var block = GetBlock(x, y, z, true);
                        
            if (block.blockCode == 0)
                return;

            v.x += worldBasePosition.x;
            v.y += worldBasePosition.y;
            v.z += worldBasePosition.z;
                        
            block.behaviorInstance.OnUpdate(
                v, 
                locatedChunkManager.gameClient.CurrentLevel,
                this,
                locatedChunkManager.gameClient.player);
        }

        public void ChunkUpdate(int cx, int cy, int cz, int width)
        {
            UpdateBlock(cx, cy + 1, cz);
            UpdateBlock(cx, cy - 1, cz);
            
            UpdateBlock(cx + 1, cy, cz);
            UpdateBlock(cx - 1, cy, cz);
            UpdateBlock(cx, cy, cz + 1);
            UpdateBlock(cx, cy, cz - 1);
        }

        public ChunkBlockData GetBlock(int x, int y, int z, bool searchLevel = false)
        {
            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);

            if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= width)
            {
                if (searchLevel)
                    return locatedChunkManager.GetBlock(worldBasePosition.x + x, 
                                                 worldBasePosition.y + y, 
                                                 worldBasePosition.z + z);
                return ChunkBlockData.EMPTY;
            }

            return chunkBlocks[x, y, z];
        }

        public ChunkBlockData GetBlock(Vector3 pos, bool searchLevel = false)
        {
            return GetBlock((int) pos.x, (int) pos.y, (int) pos.z, searchLevel);
        }
        
        public ChunkBlockData GetBlockWS(Vector3 pos, bool searchLevel = false)
        {
            var iPos = WorldToLocal((int)pos.x, (int)pos.y, (int)pos.z);;
            return GetBlock(iPos.x, iPos.y, iPos.z, searchLevel);
        }
        
        public BlockBehavior SetBlock(int blockCode, int x, int y, int z, 
                                        bool worldPos, bool updateMesh, bool updateHeightMap)
        {
            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);

            if (worldPos)
            {
                x -= worldBasePosition.x;
                z -= worldBasePosition.z;
            }

            if (x < 0 || x >= width || z >= width || z < 0 || y < 0 || y >= height)
            {
                Debug.LogWarning(
                    "SetBlock: pos out of range: (" + width + ", " + height + ", " + width + "), got (" + 
                    x + ", " + y + ", " + z + ")");
                return StaticBlockBehavior.Default;
            }

            var data = new ChunkBlockData
            {
                blockCode = blockCode,
                behaviorInstance = BlockMetaDatabase
                                        .GetBlockBehaviorByCode(blockCode)
                                            .CreateInstance()
            };
            
            chunkBlocks[x, y, z] = data;

            if (heightMap[x, z] < y && updateHeightMap)
            {
                heightMap[x, z] = y;
            }

            dirty = true;
            
            if (updateMesh)
                UpdateChuckMesh();

            return data.behaviorInstance;
        }
        
        public int GetExposedFace(int x, int y, int z)
        {
            int exposed = 0;

            if (GetBlock(x, y, z + 1).Transparent())
            {
                exposed |= 1;
            }

            if (GetBlock(x, y, z - 1).Transparent())
            {
                exposed |= 2;
            }

            if (GetBlock(x - 1, y, z).Transparent())
            {
                exposed |= 4;
            }

            if (GetBlock(x + 1, y, z).Transparent())
            {
                exposed |= 8;
            }

            if (GetBlock(x, y + 1, z).Transparent())
            {
                exposed |= 16;
            }

            if (GetBlock(x, y - 1, z).Transparent())
            {
                exposed |= 32;
            }

            return exposed;
        }
        
        public int GetExposedFaceTransparent(int x, int y, int z, int blockCode)
        {
            int exposed = 0;

            if (GetBlock(x, y, z + 1, true).blockCode != blockCode)
            {
                exposed |= 1;
            }

            if (GetBlock(x, y, z - 1, true).blockCode != blockCode)
            {
                exposed |= 2;
            }

            if (GetBlock(x - 1, y, z, true).blockCode != blockCode)
            {
                exposed |= 4;
            }

            if (GetBlock(x + 1, y, z, true).blockCode != blockCode)
            {
                exposed |= 8;
            }

            if (GetBlock(x, y + 1, z, true).blockCode != blockCode)
            {
                exposed |= 16;
            }

            if (GetBlock(x, y - 1, z, true).blockCode != blockCode)
            {
                exposed |= 32;
            }

            return exposed;
        }
        
        public int GetLightAttenuation(int x, int y, int z)
        {
            int attenuation = 0;

            if (GetHeight(x, z + 1) > y)
            {
                attenuation |= 1;
            }

            if (GetHeight(x, z - 1) > y)
            {
                attenuation |= 2;
            }

            if (GetHeight(x - 1, z) > y)
            {
                attenuation |= 4;
            }

            if (GetHeight(x + 1, z) > y)
            {
                attenuation |= 8;
            }

            if (GetHeight(x, z) > y)
            {
                attenuation |= 16;
            }

            attenuation |= 32;

            return attenuation;
        }

        public int GetHeight(int x, int z)
        {
            var width = heightMap.GetLength(0);

            if (x < 0 || x >= width || z < 0 || z >= width)
                return 0;
            return heightMap[x, z];
        }

        public IntVector3 WorldToLocal(int x, int z)
        {
            return new IntVector3(x - worldBasePosition.x, 0, z - worldBasePosition.z);
        }
        
        public IntVector3 WorldToLocal(int x, int y, int z)
        {
            return new IntVector3(x - worldBasePosition.x, y - worldBasePosition.y, z - worldBasePosition.z);
        }
    }
}