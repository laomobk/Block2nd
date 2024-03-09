using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Block2nd.Behavior;
using Block2nd.Behavior.Block;
using Block2nd.Database;
using Block2nd.MathUtil;
using UnityEngine;

namespace Block2nd.World
{
    
    public class Chunk : MonoBehaviour
    {
        public IntVector3 worldBasePosition;
        public Bounds aabb;
        
        public bool rendered = false;
        
        public ChunkBlockData[,,] chunkBlocks;
        public int[,] heightMap;

        private ChunkManager locatedChunkManager;

        private GameObject subTransparentChunk;

        public bool dirty = false;

        private void Awake()
        {
            subTransparentChunk = transform.GetChild(0).gameObject;
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
        
        public void UpdateChuckMesh()
        {
            BakeHeightMap();
            
            var opMesh = new Mesh();
            var trMesh = new Mesh();
            
            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);

            List<Vector3> opVert = new List<Vector3>();
            List<Vector2> opUvs = new List<Vector2>();
            List<Color> opColors = new List<Color>();
            List<int> opTris = new List<int>();
            
            List<Vector3> trVert = new List<Vector3>();
            List<Vector2> trUvs = new List<Vector2>();
            List<Color> trColors = new List<Color>();
            List<int> trTris = new List<int>();

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
                            if (meta.transparent)
                            {
                                tris = trTris;
                                uvs = trUvs;
                                colors = trColors;
                                vert = trVert;
                            }
                            
                            var blockMesh = behavior.OnRender();
                            if (blockMesh == null)
                                blockMesh = meta.shape.GetShapeMesh(
                                    meta.transparent ? 
                                        GetExposedFaceTransparent(x, y, z, meta.blockCode) :
                                        GetExposedFace(x, y, z),
                                    GetLightAttenuation(x, y, z));
                            var triangleStartIdx = vert.Count;

                            foreach (var triIdx in blockMesh.triangles)
                            {
                                tris.Add(triangleStartIdx + triIdx);
                            }

                            foreach (var positions in blockMesh.positions)
                            {
                                vert.Add(positions + new Vector3(x, y, z));
                            }

                            foreach (var texcoord in blockMesh.texcoords)
                            {
                                uvs.Add(texcoord);
                            }

                            foreach (var color in blockMesh.colors)
                            {
                                colors.Add(color);
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

            var curTrMesh = subTransparentChunk.GetComponent<MeshFilter>().sharedMesh;
            var curOpMesh = GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(curOpMesh, true);
            DestroyImmediate(curTrMesh, true);
            
            GetComponent<MeshFilter>().sharedMesh = opMesh;
            GetComponent<MeshCollider>().sharedMesh = opMesh;

            subTransparentChunk.GetComponent<MeshFilter>().sharedMesh = trMesh;
            subTransparentChunk.GetComponent<MeshCollider>().sharedMesh = trMesh;

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