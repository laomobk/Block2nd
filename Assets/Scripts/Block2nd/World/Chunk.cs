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
    public class Chunk
    {
        private Level level;

        public IntVector3 worldBasePosition;
        public Bounds aabb;

        public ChunkBlockData[,,] chunkBlocks;
        public int[,] heightMap = new int[16, 16];
        public byte[,,] ambientOcclusionMap;

        public bool empty = true;
        public bool dirty = true;

        public ulong CoordKey { get; }

        public Chunk(Level level, int chunkX, int chunkZ)
        {
            this.level = level;
            worldBasePosition = new IntVector3(chunkX * 16, 0, chunkZ * 16);
            CoordKey = ChunkHelper.ChunkCoordsToLongKey(chunkX, chunkZ);
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

        public void BakeAmbientOcclusionMap()
        {
            // cell: 32 位数字, 每 4 bit 为一个顶点的遮蔽数值，共 8 个顶点.
            // 从低到高分别代表顶点 B(LT, RT, LB, RB), F(LT, RT, LB, RB)
            // 只有不透明方块才会被加入到环境光遮蔽的计算之中
            
            Profiler.BeginSample("C:Bake AO");
            
            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);
            
            for (int x = 0; x < width; ++x)
            {
                for (int z = 0; z < width; ++z)
                {
                    for (int y = height - 1; y >= 0; --y)
                    {
                        byte vertAoValue = 0;

                        if (!GetBlock(x + 1, y + 1, z - 1).Transparent())
                            vertAoValue |= 1;
                        
                        if (!GetBlock(x + 1, y + 1, z).Transparent())
                            vertAoValue |= 17;
                        
                        if (!GetBlock(x + 1, y + 1, z + 1).Transparent())
                            vertAoValue |= 16;

                        if (!GetBlock(x, y + 1, z - 1).Transparent())
                            vertAoValue |= 3;
                        
                        if (!GetBlock(x, y + 1, z + 1).Transparent())
                            vertAoValue |= 48;

                        if (!GetBlock(x - 1, y + 1, z - 1).Transparent())
                            vertAoValue |= 2;
                        
                        if (!GetBlock(x - 1, y + 1, z).Transparent())
                            vertAoValue |= 34;
                        
                        if (!GetBlock(x - 1, y + 1, z + 1).Transparent())
                            vertAoValue |= 32;
                        
                        
                        if (!GetBlock(x + 1, y - 1, z - 1).Transparent())
                            vertAoValue |= 4;
                        
                        if (!GetBlock(x + 1, y - 1, z).Transparent())
                            vertAoValue |= 68;
                        
                        if (!GetBlock(x + 1, y - 1, z + 1).Transparent())
                            vertAoValue |= 64;

                        if (!GetBlock(x, y - 1, z - 1).Transparent())
                            vertAoValue |= 12;
                        
                        if (!GetBlock(x, y - 1, z + 1).Transparent())
                            vertAoValue |= 192;

                        if (!GetBlock(x - 1, y - 1, z - 1).Transparent())
                            vertAoValue |= 8;
                        
                        if (!GetBlock(x - 1, y - 1, z).Transparent())
                            vertAoValue |= 136;
                        
                        if (!GetBlock(x - 1, y - 1, z + 1).Transparent())
                            vertAoValue |= 128;
                        
                        
                        if (!GetBlock(x - 1, y, z + 1).Transparent())
                            vertAoValue |= 80;
                        
                        if (!GetBlock(x - 1, y, z - 1).Transparent())
                            vertAoValue |= 5;
                        
                        
                        if (!GetBlock(x - 1, y, z + 1).Transparent())
                            vertAoValue |= 128;
                        
                        if (!GetBlock(x - 1, y, z - 1).Transparent())
                            vertAoValue |= 160;

                        ambientOcclusionMap[x, y, z] = vertAoValue;
                    }
                }
            }
            
            Profiler.EndSample();
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
                        
            BlockMetaDatabase.GetBlockBehaviorByCode(block.blockCode).OnUpdate(
                v, 
                level,
                this,
                level.Player);
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
                    return level.GetBlock(worldBasePosition.x + x, 
                                                 worldBasePosition.y + y, 
                                                 worldBasePosition.z + z);
                return ChunkBlockData.EMPTY;
            }

            return chunkBlocks[x, y, z];
        }

        public ChunkBlockData GetBlock(IntVector3 pos, bool searchLevel = false)
        {
            return GetBlock(pos.x, pos.y, pos.z, searchLevel);
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
        
        public void SetBlock(int blockCode, int x, int y, int z, 
                                        bool worldPos, bool updateMesh, bool updateHeightMap, byte state)
        {
            int ox = x, oy = y, oz = z;
            
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
                return;
            }

            var data = new ChunkBlockData
            {
                blockCode = blockCode,
                blockState = state
            };

            chunkBlocks[x, y, z] = data;
            

            if (heightMap[x, z] < y && updateHeightMap)
            {
                heightMap[x, z] = y;
            }

            dirty = true;
        }

        public void SetBlockState(int x, int y, int z, byte state, bool worldPos, bool updateMesh)
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
                    "SetBlockState: pos out of range: (" + width + ", " + height + ", " + width + "), got (" + 
                    x + ", " + y + ", " + z + ")");
                return;
            }

            chunkBlocks[x, y, z].blockState = state;

            dirty = true;
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
        
        public IntVector3 LocalToWorld(int x, int y, int z)
        {
            return new IntVector3(x + worldBasePosition.x, y + worldBasePosition.y, z + worldBasePosition.z);
        }
    }
}