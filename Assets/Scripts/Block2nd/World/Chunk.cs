
// #define CHK_LIGHT_DEBUG 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Block2nd.Behavior;
using Block2nd.Behavior.Block;
using Block2nd.Client;
using Block2nd.Client.GameDebug;
using Block2nd.Entity;
using Block2nd.Database;
using Block2nd.MathUtil;
using Block2nd.Persistence.KNBT;
using Block2nd.Scriptable;
using Block2nd.UnsafeStructure;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Block2nd.World
{
    public class Chunk
    {
        private int chunkHeight;
        private Level level;

        public IntVector3 worldBasePosition;
        public Bounds aabb;

        public VerticalList<List<EntityBase>> entityStorage; 
        
        public ChunkBlockData[,,] chunkBlocks;
        public int[,] heightMap = new int[16, 16];
        public int[,,] lightMap;
        
        public int[] skyLightMap;
        public int[] blockLightMap;

        public bool modified = true;
        public bool dirty = true;
        public bool firstRendered = false;
        public bool saved;
        
        public int populateState = 0;
        public bool lightened = false;

        public ulong CoordKey { get; }

        public bool NeedToSave => !saved || modified;

        public Chunk(Level level, int chunkX, int chunkZ, int chunkHeight)
        {
            this.level = level;
            worldBasePosition = new IntVector3(chunkX * 16, 0, chunkZ * 16);
            CoordKey = ChunkHelper.ChunkCoordsToLongKey(chunkX, chunkZ);

            lightMap = new int[16, chunkHeight, 16];
            
            skyLightMap = new int[16 * chunkHeight * 16];
            blockLightMap = new int[16 * chunkHeight * 16];
            
            entityStorage = new VerticalList<List<EntityBase>>(chunkHeight / 16 + 1, 16, 16, 16);

            this.chunkHeight = chunkHeight;
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

        public void BakeHeightMapPartial(int x, int z)
        {
            var height = chunkBlocks.GetLength(1);

            for (int y = height - 1; y >= 0; --y)
            {
                if (chunkBlocks[x, y, z].blockCode != 0)
                {
                    heightMap[x, z] = y;
                    break;
                }
            }
        }

        public void BakeHeightMap()
        {
            if (!dirty)
                return;

            Profiler.BeginSample("Bake HeightMap");

            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);

            for (int x = 0; x < width; ++x)
            {
                for (int z = 0; z < width; ++z)
                {
                    for (int y = height - 1; y >= 0; --y)
                    {
                        var code = chunkBlocks[x, y, z].blockCode;
                        if (code != 0 && (BlockMetaDatabase.types[code] & BlockTypeBits.PlantBit) == 0)
                        {
                            heightMap[x, z] = y;
                            break;
                        }
                    }
                }
            }

            Profiler.EndSample();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CalcLightMapIndex(int x, int y, int z)
        {
            return (y << 4) | (z << 2) | x;
        }
        
        public void BakeHeightMapWithSkyLightUpdate()
        {
            // TODO: reset this check.
            /*
            if (!dirty)
                return;
                */

            Profiler.BeginSample("Bake HeightMap");

            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);

            for (int x = 0; x < width; ++x)
            {
                for (int z = 0; z < width; ++z)
                {
                    for (int y = height - 1; y >= 0; --y)
                    {
                        var code = chunkBlocks[x, y, z].blockCode;
                        if (code != 0 && (BlockMetaDatabase.types[code] & BlockTypeBits.PlantBit) == 0)
                        {
                            heightMap[x, z] = y;
                            break;
                        }

                        skyLightMap[CalcLightMapIndex(x, y, z)] = 15;
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

        public ChunkBlockData GetBlock(int x, int y, int z, bool searchLevel = false, bool cacheOnly = true)
        {
            var width = chunkBlocks.GetLength(0);
            var height = chunkBlocks.GetLength(1);

            if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= width)
            {
                if (searchLevel)
                    return level.GetBlock(worldBasePosition.x + x,
                        worldBasePosition.y + y,
                        worldBasePosition.z + z,
                        false, cacheOnly);
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
            var iPos = WorldToLocal((int) pos.x, (int) pos.y, (int) pos.z);
            ;
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

        public int GetLightAttenuation(int x, int y, int z, int exposedFace)
        {
            int attenuation = 0;

            if ((exposedFace & 1) != 0)
            {
                attenuation |= GetSkyLight(x, y, z + 1);
            }

            if ((exposedFace & 2) != 0)
            {
                attenuation |= GetSkyLight(x, y, z - 1) << 4;
            }

            if ((exposedFace & 4) != 0)
            {
                attenuation |= GetSkyLight(x - 1, y, z) << 8;
            }

            if ((exposedFace & 8) != 0)
            {
                attenuation |= GetSkyLight(x + 1, y, z) << 12;
            }

            if ((exposedFace & 16) != 0)
            {
                attenuation |= GetSkyLight(x, y + 1, z) << 16;
            }

            if ((exposedFace & 32) != 0)
            {
                attenuation |= GetSkyLight(x, y - 1, z) << 20;
            }

            return attenuation;
        }

        public int GetLightAttenuation(int x, int y, int z)
        {
            int attenuation = 0;

            if (GetHeight(x, z + 1) > y)
            {
                attenuation |= 2;
            }

            if (GetHeight(x, z - 1) > y)
            {
                attenuation |= 2 << 4;
            }

            if (GetHeight(x - 1, z) > y)
            {
                attenuation |= 2 << 8;
            }

            if (GetHeight(x + 1, z) > y)
            {
                attenuation |= 2 << 12;
            }

            if (GetHeight(x, z) > y)
            {
                attenuation |= 2 << 16;
            }

            attenuation |= 2 << 20;

            return attenuation;
        }

        public int GetHeight(int x, int z, bool searchLevel = false, bool cacheOnly = false)
        {
            var width = heightMap.GetLength(0);

            if (x < 0 || x >= width || z < 0 || z >= width)
            {
                if (searchLevel)
                {
                    return level.GetHeight(x + worldBasePosition.x, z + worldBasePosition.z, cacheOnly);
                }

                return 0;
            }

            return heightMap[x, z];
        }

        public int GetSkyLight(int x, int y, int z, bool cacheOnly = false)
        {
            var height = chunkBlocks.GetLength(1);

            if (y >= height || y < 0)
                return 0;

            if (x < 0 || x >= 16 || z < 0 || z >= 16)
            {
                return level.GetSkyLight(x + worldBasePosition.x, y, z + worldBasePosition.z, cacheOnly);
            }

            return skyLightMap[(y << 4) + (x << 2) + z];
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

        public KNBTTagCompound GetChunkKNBTData()
        {
            var tree = new KNBTTagCompound("Chunk");

            tree.SetInt("Height", chunkBlocks.GetLength(1));
            tree.SetChunkBlockDataTensor("Blocks", chunkBlocks);
            tree.SetInt("PopulateState", populateState);

            return tree;
        }

        public void UpdateChunkLightMapFully()
        {
            Profiler.BeginSample("Update Lightmap Fully");

            Array.Clear(lightMap, 0, 16 * 16 * level.worldSettings.chunkHeight);

            for (int x = 0; x < 16; ++x)
            {
                for (int z = 0; z < 16; ++z)
                {
                    int groundHeight = heightMap[x, z];

                    for (int y = groundHeight; y >= 0; --y)
                    {
                        if (GetBlock(x, y, z).Transparent())
                        {
                            int value = 3;
                            if (GetHeight(x - 1, z, true) > y &&
                                GetHeight(x + 1, z, true) > y &&
                                GetHeight(x, z + 1, true) > y &&
                                GetHeight(x, z - 1, true) > y)
                            {
                                value = 6;
                            }

                            lightMap[x, y, z] = value;
                        }
                    }
                }
            }

            Profiler.EndSample();
        }
        
        public void UpdateChunkLightMapFullyNew()
        {
            BakeHeightMap();
            
            Profiler.BeginSample("Update Lightmap Fully");

            Array.Clear(lightMap, 0, 16 * 16 * level.worldSettings.chunkHeight);

            var lightUpdateUnitQueue = new Queue<LightUpdateUnit>();
            for (int i = 0; i < 256; ++i)
            {
                UpdateLightQueueForSkyLight(i % 16, i / 16, lightUpdateUnitQueue);
            }
            UpdateLightByType(lightUpdateUnitQueue, LightType.SKY);

            Profiler.EndSample();
        }

        public int GetSavedLightValueByType(int x, int y, int z, LightType type)
        {
            int idx = (y << 4) + (z << 2) + x;

            if (y < 0)
                return 0;

            if (x < 0 || x >= 16 || z < 0 || z >= 16)
            {
                var worldPos = LocalToWorld(x, y, z);
                var chk = level.GetChunkFromCoords(worldPos.x >> 4, worldPos.z >> 4);
                var nearLocal = chk.WorldToLocal(worldPos.x, worldPos.y, worldPos.z);
                return chk.GetSavedLightValueByType(nearLocal.x, nearLocal.y, nearLocal.z, type);
            }

            if (idx >= skyLightMap.Length)
                return 15;

            switch (type)
            {
                case LightType.SKY: return skyLightMap[idx];
                case LightType.BLOCK: return blockLightMap[idx];
            }
            
            return 0;
        }
        
        public void SetLightValueByType(int x, int y, int z, LightType type, int lightValue)
        {
            int idx = (y << 4) + (z << 2) + x;
            
            if (y < 0 || idx >= skyLightMap.Length)
                return;

            if (x < 0 || x >= 16 || z < 0 || z >= 16)
            {
                var worldPos = LocalToWorld(x, y, z);
                var chk = level.GetChunkFromCoords(worldPos.x >> 4, worldPos.z >> 4);
                var nearLocal = chk.WorldToLocal(worldPos.x, worldPos.y, worldPos.z);
                chk.SetLightValueByType(nearLocal.x, nearLocal.y, nearLocal.z, type, lightValue);
                return;
            }

            if (lightValue < 0)
                lightValue = 0;
            else if (lightValue > 15)
                lightValue = 15;

            switch (type)
            {
                case LightType.SKY: skyLightMap[idx] = lightValue; break;
                case LightType.BLOCK: blockLightMap[idx] = lightValue; break;
            }
        }

        // TODO: delete this debug api
        private void PutDebugBall(int chunkWorldX, int chunkWorldZ, int x, int y, int z, 
                                    Color color, float aliveTime = 999, string label = "")
        {
            if (worldBasePosition.x == chunkWorldX && worldBasePosition.z == chunkWorldZ)
            {
                var go = GameClientDebugger.Instance.CreateDebugObject(new Vector3(x, y, z) + 
                                                              worldBasePosition.ToUnityVector3(), color, label);
                GameObject.Destroy(go, aliveTime);
            }
        }
        
        // TODO: delete this debug api
        public IEnumerator DebugLightUpdate()
        {
            var lightUpdateUnitQueue = new Queue<LightUpdateUnit>();
            for (int i = 0; i < 256; ++i)
            {
                UpdateLightQueueForSkyLight(i % 16, i / 16, lightUpdateUnitQueue);
            }
            
#if CHK_LIGHT_DEBUG
            return UpdateLightByType(lightUpdateUnitQueue, LightType.SKY);
#else
            yield break;
#endif
        }

        private void UpdateLightQueueForSkyLight(int lx, int lz, Queue<LightUpdateUnit> queue)
        {
            int height = GetHeight(lx, lz);

            int heightR = GetHeight(lx + 1, lz, true),
                heightL = GetHeight(lx - 1, lz, true),
                heightF = GetHeight(lx, lz + 1, true),
                heightB = GetHeight(lx, lz - 1, true);
            
            for (int y = height + 1; y < chunkHeight; ++y)
            {
                if (heightR > y)
                {
                    queue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx + 1,
                        localY = y,
                        localZ = lz,
                    });
                }
                
                if (heightL > y)
                {
                    queue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx - 1,
                        localY = y,
                        localZ = lz,
                    });
                }
                
                if (heightF > y)
                {
                    queue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx,
                        localY = y,
                        localZ = lz + 1,
                    });
                }
                
                if (heightB > y)
                {
                    queue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx,
                        localY = y,
                        localZ = lz - 1,
                    });
                }

                SetLightValueByType(lx, y, lz, LightType.SKY, 15);
            }
        }

        private void UpdateLightQueueForBlockLightFully(Queue<LightUpdateUnit> queue)
        {
            for (int x = 0; x < 16; ++x)
            {
                for (int z = 0; z < 16; ++z)
                {
                    for (int y = 0; y < chunkHeight; ++y)
                    {
                        var blockData = chunkBlocks[x, y, z];
                        var lightEffect = BlockMetaDatabase.GetBlockMetaByCode(blockData.blockCode).opacity;

                        if (lightEffect > 0)
                        {
                            queue.Enqueue(new LightUpdateUnit
                            {
                                localX = x,
                                localY = y,
                                localZ = z,
                            });
                        }
                        
                        SetLightValueByType(x, y, y, LightType.BLOCK, 15);
                    }
                }
            }
        }

#if  CHK_LIGHT_DEBUG
        private IEnumerator UpdateLightByType(Queue<LightUpdateUnit> lightUpdateUnitQueue, LightType lightType)
        {
#else
        private void UpdateLightByType(Queue<LightUpdateUnit> lightUpdateUnitQueue, LightType lightType)
        {
#endif
        
            var tick = 0;
            
#if CHK_LIGHT_DEBUG
            yield return null;
#endif

            while (lightUpdateUnitQueue.Count > 0)
            {
                if (tick > 10000)
                {
                    Debug.LogWarning("UpdateLightByType: too many light update unit!");
                    break;
                }
                
                var unit = lightUpdateUnitQueue.Dequeue();
                int lx = unit.localX, ly = unit.localY, lz = unit.localZ;

                var blockCode = GetBlock(lx, ly, lz, true).blockCode;
                var curBlockOpacity = BlockMetaDatabase.GetBlockOpacityByCode(blockCode);
                var curBlockLight = BlockMetaDatabase.GetBlockLightByCode(blockCode);
                
                var curLightValue = GetSavedLightValueByType(lx, ly, lz, lightType);

                if (curBlockOpacity > 16)
                {
                    continue;
                }

                var computedLightValue = ComputeAndGetBlockLightNearBy(
                    lx, ly, lz, curBlockOpacity, curBlockLight, lightType,
                    out int lightRight, out int lightLeft,
                    out int lightAbove, out int lightBelow,
                    out int lightAhead, out int lightBehind);
                
#if CHK_LIGHT_DEBUG
                PutDebugBall(208, 128, lx, ly, lz, Color.red, 0.1f, 
                    computedLightValue + " C: " + curLightValue + " O: " + curBlockOpacity + " B: " + blockCode);
#endif
                if (computedLightValue == curLightValue)
                    continue; // don't need to spread light
                
                SetLightValueByType(lx, ly, lz, lightType, computedLightValue);

                if (lightRight < computedLightValue)
                {
                    lightUpdateUnitQueue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx + 1, localY = ly, localZ = lz
                    });
                    
#if CHK_LIGHT_DEBUG
                    PutDebugBall(208, 128, lx + 1, ly, lz, Color.green, 0.1f,
                        "l: " + lightRight);
#endif
                }
                if (lightLeft < computedLightValue)
                {
                    lightUpdateUnitQueue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx - 1, localY = ly, localZ = lz
                    });
                    
#if CHK_LIGHT_DEBUG
                    PutDebugBall(208, 128, lx - 1, ly, lz, Color.green, 0.1f,
                        "l: " + lightLeft);
#endif
                }
                if (lightAbove < computedLightValue)
                {
                    lightUpdateUnitQueue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx, localY = ly + 1, localZ = lz
                    });

#if CHK_LIGHT_DEBUG
                    PutDebugBall(208, 128, lx, ly + 1, lz, Color.green, 0.1f,
                        "l: " + lightAbove);
#endif
                }
                if (lightBelow < computedLightValue)
                {
                    lightUpdateUnitQueue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx, localY = ly - 1, localZ = lz
                    });
                    
#if CHK_LIGHT_DEBUG
                    PutDebugBall(208, 128, lx, ly - 1, lz, Color.green, 0.1f,
                        "l: " + lightBelow);
#endif
                }
                if (lightAhead < computedLightValue)
                {
                    lightUpdateUnitQueue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx, localY = ly, localZ = lz + 1
                    });
                    
#if CHK_LIGHT_DEBUG
                    PutDebugBall(208, 128, lx, ly, lz + 1, Color.green, 0.1f,
                        "l: " + lightAhead);
#endif
                }
                if (lightBehind < computedLightValue)
                {
                    lightUpdateUnitQueue.Enqueue(new LightUpdateUnit
                    {
                        localX = lx, localY = ly, localZ = lz - 1
                    });
                    
#if CHK_LIGHT_DEBUG
                    PutDebugBall(208, 128, lx, ly, lz - 1, Color.green, 0.1f,
                        "l: " + lightBehind);
#endif
                }

                tick++;

#if CHK_LIGHT_DEBUG
                yield return null;
#endif
            }
            
#if CHK_LIGHT_DEBUG
            yield return null;
#endif
        }
        
        private int ComputeAndGetBlockLightNearBy(int x, int y, int z, int opacity, int light, LightType lightType,
                                                    out int lightRight, out int lightLeft,
                                                    out int lightAbove, out int lightBelow,
                                                    out int lightAhead, out int lightBehind)
        {
            if (lightType == LightType.SKY && y > GetHeight(x, z, true))
            {
                // Debug.Log(x + ", " + y + ", " + z);
                light = 15;
            }

            if (opacity == 0)
                opacity = 1;

            lightRight = GetSavedLightValueByType(x + 1, y, z, lightType);
            lightLeft = GetSavedLightValueByType(x - 1, y, z, lightType);
            lightAbove = GetSavedLightValueByType(x, y + 1, z, lightType);
            lightBelow = GetSavedLightValueByType(x, y - 1, z, lightType);
            lightAhead = GetSavedLightValueByType(x, y, z + 1, lightType);
            lightBehind = GetSavedLightValueByType(x, y, z - 1, lightType);
            
            return Mathf.Max(lightRight - opacity, 
                lightLeft - opacity, 
                lightAbove - opacity, 
                lightBelow - opacity, 
                lightAhead - opacity, 
                lightBehind - opacity, light, 0);
        }

        public void UpdateChunkSkylightForBlock(int x, int y, int z)
        {
            BakeHeightMapPartial(x, z);
            var height = GetHeight(x, z);

            if (y == height)
            {
                for (; y >= 0; --y)
                {
                    if (GetBlock(x, y, z).Transparent())
                    {
                        int value = 3;
                        if (GetHeight(x - 1, z, true, true) > y &&
                            GetHeight(x + 1, z, true, true) > y &&
                            GetHeight(x, z + 1, true, true) > y &&
                            GetHeight(x, z - 1, true, true) > y)
                        {
                            value = 6;
                        }

                        lightMap[x, y, z] = value;
                    }
                }
            }
        }

        private void UpdateChunkSkyLight()
        {
            
        }

        public void AddEntity(EntityBase entity, float worldX, float y, float worldZ)
        {
            var x = Mathf.FloorToInt(worldX - worldBasePosition.x);
            var z = Mathf.FloorToInt(worldZ - worldBasePosition.z);
            
            var cell = entityStorage.Get((int)y >> 4);

            var localY = Mathf.FloorToInt(y - y / 16 * 16);

            var list = cell[x, localY, z];
            
            if (list == null)
            {
                var newList = new List<EntityBase>();
                cell[x, localY, z] = newList;

                list = newList;
            }

            list.Add(entity);
        }
    }
}