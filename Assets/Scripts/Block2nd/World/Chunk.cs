
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
        public int chunkHeight;
        private Level level;

        public IntVector3 worldBasePosition;
        public Bounds aabb;

        public VerticalList<List<EntityBase>> entityStorage; 
        
        public ChunkBlockData[,,] chunkBlocks;
        public int[,] heightMap = new int[16, 16];
        public int[,,] lightMap;
        
        public byte[] skyLightMap;
        public byte[] blockLightMap;
        
        public bool[] cleanSkyLightColumn = new bool[256];
        public int[] dirtyColumnRange = new int[256];

        public bool modified = true;
        public bool dirty = true;
        public bool firstRendered = false;
        public bool saved;
        
        public int populateState = 0;

        public int lightingState = 0;

        public ulong CoordKey { get; }

        public bool NeedToSave => !saved || modified;

        // high [LF][LB][RF][RB][L][R][B][F] low
        public int lightUpdateSurroundingBits = 0;

        private static int[] lightUpdateQueue = new int[32768];

        public Chunk(Level level, int chunkX, int chunkZ, int chunkHeight)
        {
            this.level = level;
            worldBasePosition = new IntVector3(chunkX * 16, 0, chunkZ * 16);
            CoordKey = ChunkHelper.ChunkCoordsToLongKey(chunkX, chunkZ);

            lightMap = new int[16, chunkHeight, 16];
            
            skyLightMap = new byte[16 * chunkHeight * 16];
            blockLightMap = new byte[16 * chunkHeight * 16];
            
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
                        if (chunkBlocks[x, y, z].IsSolid())
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
            return (y << 8) | (z << 4) | x;
        }
        
        public void BakeHeightMapWithSkyLightUpdate()
        {
            Profiler.BeginSample("Bake HeightMap With SkyLight Update");

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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ChunkBlockData GetBlockFast(int x, int y, int z)
        {
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

            RelightBlocksInGap(x, y, z);

            if (updateHeightMap &&  
                blockCode != 0 && (BlockMetaDatabase.types[blockCode] & BlockTypeBits.PlantBit) == 0
                && heightMap[x, z] < y)
            {
                heightMap[x, z] = y;
            }

            dirty = true;
            modified = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Calc2DMapIndex(int x, int y)
        {
            return x << 4 | y;
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

        public long GetLightAttenuation(int x, int y, int z, int exposedFace)
        {
            long attenuation = 0;

            if ((exposedFace & 1) != 0)
            {
                attenuation |= GetSkyLight(x, y, z + 1) | ((long)GetBlockLight(x, y, z + 1) << 24);
            }

            if ((exposedFace & 2) != 0)
            {
                attenuation |= (GetSkyLight(x, y, z - 1) << 4) | ((long)GetBlockLight(x, y, z - 1) << 28);
            }

            if ((exposedFace & 4) != 0)
            {
                attenuation |= (GetSkyLight(x - 1, y, z) << 8) | ((long)GetBlockLight(x - 1, y, z) << 32);
            }

            if ((exposedFace & 8) != 0)
            {
                attenuation |= (GetSkyLight(x + 1, y, z) << 12) | ((long)GetBlockLight(x + 1, y, z) << 36);
            }

            if ((exposedFace & 16) != 0)
            {
                attenuation |= (GetSkyLight(x, y + 1, z) << 16) | ((long)GetBlockLight(x, y + 1, z) << 40);
            }

            if ((exposedFace & 32) != 0)
            {
                attenuation |= (GetSkyLight(x, y - 1, z) << 20) | ((long)GetBlockLight(x, y - 1, z) << 44);
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHeightFast(int x, int z)
        {
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

            return skyLightMap[CalcLightMapIndex(x, y, z)];
        }
        
        public int GetBlockLight(int x, int y, int z, bool cacheOnly = false)
        {
            var height = chunkBlocks.GetLength(1);

            if (y >= height || y < 0)
                return 0;
            
            if (x < 0 || x >= 16 || z < 0 || z >= 16)
            {
                return level.GetBlockLight(x + worldBasePosition.x, y, z + worldBasePosition.z, cacheOnly);
            }

            return blockLightMap[CalcLightMapIndex(x, y, z)];
        }

        public bool CanThisBlockSeeTheSky(int cx, int cy, int cz)
        {
            int height = GetHeight(cx, cz);

            return cy >= height;
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
            
            tree.SetByteArray("SkyLightMap", skyLightMap);
            tree.SetByteArray("BlockLightMap", blockLightMap);
            
            tree.SetInt("PopulateState", populateState);
            tree.SetInt("LightingState", lightingState);

            return tree;
        }

        public bool SetChunkWithKNBTData(KNBTTagCompound data)
        {
            chunkHeight = data.GetInt("Height", 128);
            
            populateState = data.GetInt("PopulateState");
            lightingState = data.GetInt("LightingState");
            
            var skyLightMapRead = data.GetByteArray("SkyLightMap");
            var blockLightMapRead = data.GetByteArray("BlockLightMap");

            if (skyLightMapRead != null)
            {
                skyLightMap = skyLightMapRead;
            }

            if (blockLightMapRead != null)
            {
                blockLightMap = blockLightMapRead;
            }

            if (lightingState >= 2)
            {
                for (int i = 0; i < 256; ++i)
                    cleanSkyLightColumn[i] = true;
            }
            
            chunkBlocks = data.GetChunkBlockDataTensor("Blocks");
            if (chunkBlocks == null)
                return false;
            return true;
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
            /*  do not check surrounding anymore.
            if (!IsAroundChunksInCache())
            {
                Debug.LogWarning(
                    $"cannot update lighting due to the lack of surrounding chunks: " +
                    $"{worldBasePosition} ({worldBasePosition.ToChunkCoordPos()})");
                return;
            }
            */

            BakeHeightMap();

            Profiler.BeginSample("Update Lightmap Fully");

            Array.Clear(lightMap, 0, 16 * 16 * level.worldSettings.chunkHeight);

            CheckAndUpdateSkyLights();
            UpdateBlockLights(true);

            lightingState = 2;

            Profiler.EndSample();
        }

        public void UpdateBlockLights(bool withSurroundingChunkCheck = false)
        {
            lightUpdateSurroundingBits = 0;
            
            for (int x = 0; x < 16; ++x)
            {
                for (int z = 0; z < 16; ++z)
                {
                    var height = heightMap[x, z];
                    for (int y = height; y >= 0; --y)
                    {
                        var blockLight = BlockMetaDatabase.GetBlockMetaByCode(
                            chunkBlocks[x, y, z].blockCode)?.light;
                        
                        if (blockLight != null && blockLight > 0)
                        {
                            UpdateLightByType(x, y, z, LightType.BLOCK);
                        }
                    }
                }
            }
            
            if (withSurroundingChunkCheck)
                level.ScheduleSurroundingChunkUpdateForLightUpdate(
                    lightUpdateSurroundingBits, 
                    worldBasePosition.x >> 4, 
                    worldBasePosition.z >> 4);
        }

        public void CheckAndUpdateSkyLights(bool withSurroundingChunkCheck = false)
        {
            lightUpdateSurroundingBits = 0;
            
            for (int x = 0; x < 16; ++x)
            {
                for (int z = 0; z < 16; ++z)
                {
                    int idx = Calc2DMapIndex(x, z);
                    if (cleanSkyLightColumn[idx])
                    {
                        continue;
                    }

                    cleanSkyLightColumn[idx] = true;

                    if (dirtyColumnRange[idx] > 0)
                    {
                        int range = dirtyColumnRange[idx];
                        int heightA = range & 0xff, heightB = (range >> 8) & 0xff;
                        UpdateSkyLightGap(x, z, heightA, heightB);
                        
                        dirtyColumnRange[idx] = 0;
                    }
                    else
                    {
                        UpdateLightQueueForSkyLight(x, z);
                    }
                }
            }
            
            if (withSurroundingChunkCheck)
                level.ScheduleSurroundingChunkUpdateForLightUpdate(
                    lightUpdateSurroundingBits, 
                    worldBasePosition.x >> 4, 
                    worldBasePosition.z >> 4);
        }

        public int GetSavedLightValueByType(int x, int y, int z, LightType type)
        {
            int idx = CalcLightMapIndex(x, y, z);

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
            {
                return 15;
            }

            switch (type)
            {
                case LightType.SKY: return skyLightMap[idx];
                case LightType.BLOCK: return blockLightMap[idx];
            }
            
            return 0;
        }
        
        public void SetLightValueByType(int x, int y, int z, LightType type, int lightValue)
        {
            int idx = CalcLightMapIndex(x, y, z);
            
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
                case LightType.SKY: skyLightMap[idx] = (byte) lightValue; break;
                case LightType.BLOCK: blockLightMap[idx] = (byte) lightValue; break;
            }

            modified = true;
        }
        
        // TODO: delete this debug api
        public IEnumerator DebugLightUpdate()
        {
            for (int i = 0; i < 256; ++i)
            {
                UpdateLightQueueForSkyLight(i % 16, i / 16);
            }
            level.ChunkRenderEntityManager.RenderChunk(this, true);
            yield break;
        }

        private void UpdateLightQueueForSkyLight(int cx, int cz)
        {
            int height = GetHeight(cx, cz);

            int heightR = GetHeight(cx + 1, cz, true),
                heightL = GetHeight(cx - 1, cz, true),
                heightF = GetHeight(cx, cz + 1, true),
                heightB = GetHeight(cx, cz - 1, true);

            int minHeight = heightR;
            if (heightL < minHeight) minHeight = heightL;
            if (heightF < minHeight) minHeight = heightF;
            if (heightB < minHeight) minHeight = heightB;
            
            UpdateSkyLightGap(cx + 1, cz, heightR, height);
            UpdateSkyLightGap(cx - 1, cz, heightL, height);
            UpdateSkyLightGap(cx, cz + 1, heightF, height);
            UpdateSkyLightGap(cx, cz - 1, heightB, height);
            UpdateSkyLightGap(cx, cz, height, minHeight);
        }

        private void UpdateSkyLightGap(int cx, int cz, int heightA, int heightB)
        {
            int from = heightA, to = heightB;
            if (heightA > heightB)
            {
                from = heightB;
                to = heightA;
            }
            
            if (cx < 0 || cx >= 16 || cz < 0 || cz >= 16)
            {
                var worldPos = LocalToWorld(cx, 0, cz);
                var chk = level.GetChunkFromCoords(worldPos.x >> 4, worldPos.z >> 4);
                var nearLocal = chk.WorldToLocal(worldPos.x, worldPos.y, worldPos.z);
                chk.UpdateSkyLightGap(nearLocal.x, nearLocal.z, heightA, heightB);
                return;
            }

            for (int y = from; y <= to; ++y)
            {
                UpdateLightByType(cx, y, cz, LightType.SKY);
            }
        }
        
        private int ComputeBlockLightNearBy(int cx, int cy, int cz, int opacity, int light, LightType lightType)
        {
            if (opacity == 0)
                opacity = 1;

            int lightRight = GetSavedLightValueByType(cx + 1, cy, cz, lightType);
            int lightLeft = GetSavedLightValueByType(cx - 1, cy, cz, lightType);
            int lightAbove = GetSavedLightValueByType(cx, cy + 1, cz, lightType);
            int lightBelow = GetSavedLightValueByType(cx, cy - 1, cz, lightType);
            int lightAhead = GetSavedLightValueByType(cx, cy, cz + 1, lightType);
            int lightBehind = GetSavedLightValueByType(cx, cy, cz - 1, lightType);
            
            if (lightType == LightType.SKY)
            {
                if (cy > GetHeight(cx, cz, true))
                    return 15;
                light = 0;  // don't consider the block itself light value.
            }

            return Mathf.Max(lightRight - opacity, 
                lightLeft - opacity, 
                lightAbove - opacity, 
                lightBelow - opacity, 
                lightAhead - opacity, 
                lightBehind - opacity, light, 0);
        }
        
        private int ComputeAndGetBlockLightNearBy(int cx, int cy, int cz, int opacity, int light, LightType lightType,
            out int lightRight, out int lightLeft,
            out int lightAbove, out int lightBelow,
            out int lightAhead, out int lightBehind)
        {

            if (opacity == 0)
                opacity = 1;

            lightRight = GetSavedLightValueByType(cx + 1, cy, cz, lightType);
            lightLeft = GetSavedLightValueByType(cx - 1, cy, cz, lightType);
            lightAbove = GetSavedLightValueByType(cx, cy + 1, cz, lightType);
            lightBelow = GetSavedLightValueByType(cx, cy - 1, cz, lightType);
            lightAhead = GetSavedLightValueByType(cx, cy, cz + 1, lightType);
            lightBehind = GetSavedLightValueByType(cx, cy, cz - 1, lightType);
            
            if (lightType == LightType.SKY)
            {
                if (cy > GetHeight(cx, cz, true))
                    return 15;
                light = 0;
            }

            return Mathf.Max(lightRight - opacity, 
                lightLeft - opacity, 
                lightAbove - opacity, 
                lightBelow - opacity, 
                lightAhead - opacity, 
                lightBehind - opacity, light, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PackLightUpdateEvent(int dx, int dy, int dz, int lightValue = 0)
        {
            dx += 32;
            dy += 32;
            dz += 32;
            return (lightValue << 18) | (dz << 12) | (dy << 6) | dx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UnpackLightUpdateEvent(int packedEvent, out int dx, out int dy, out int dz, out int lightValue)
        {
            dx = (packedEvent & 0x3f) - 32;
            dy = ((packedEvent >> 6) & 0x3f) - 32;
            dz = ((packedEvent >> 12) & 0x3f) - 32;
            lightValue = (packedEvent >> 18) & 0x3f;
        }

        public bool IsAroundChunksInCache()
        {
            int cx = worldBasePosition.x >> 4;
            int cz = worldBasePosition.z >> 4;

            return level.GetChunkFromCoords(cx + 1, cz, true) != null && 
                   level.GetChunkFromCoords(cx - 1, cz, true) != null &&
                   level.GetChunkFromCoords(cx, cz + 1, true) != null &&
                   level.GetChunkFromCoords(cx, cz - 1, true) != null &&
                   level.GetChunkFromCoords(cx - 1, cz - 1, true) != null &&
                   level.GetChunkFromCoords(cx - 1, cz + 1, true) != null &&
                   level.GetChunkFromCoords(cx + 1, cz - 1, true) != null &&
                   level.GetChunkFromCoords(cx + 1, cz + 1, true) != null;
        }

        public IntVector3[] GetAroundChunksPosList()
        {
                int cx = worldBasePosition.x >> 4;
                int cz = worldBasePosition.z >> 4;

                return new[]
                {
                    new IntVector3(cx + 1, cz),
                    new IntVector3(cx - 1, cz),
                    new IntVector3(cx, cz + 1),
                    new IntVector3(cx, cz - 1),
                    new IntVector3(cx - 1, cz - 1),
                    new IntVector3(cx - 1, cz + 1),
                    new IntVector3(cx + 1, cz - 1),
                    new IntVector3(cx + 1, cz + 1)
                };
        }

        public void UpdateAllLightType(int cx, int cy, int cz)
        {
            UpdateLightByType(cx, cy, cz, LightType.SKY);
            UpdateLightByType(cx, cy, cz, LightType.BLOCK);
        }
        
#if UNITY_EDITOR
        public void UpdateLightByType(int cx, int cy, int cz, LightType lightType, bool verbose = false)
        {
#else
        public void UpdateLightByType(int cx, int cy, int cz, LightType lightType)
        {
#endif

            Profiler.BeginSample("Update Light By Type");

            bool afterReCalcUpdate = false;
            
            int queueHead = 0, queueBack = 0;

            var blockCode = GetBlock(cx, cy, cz, true, false).blockCode;
            var curBlockOpacity = BlockMetaDatabase.GetBlockOpacityByCode(blockCode);
            var curBlockLight = BlockMetaDatabase.GetBlockLightByCode(blockCode);

            var computedLightValue = ComputeBlockLightNearBy(
                cx, cy, cz, curBlockOpacity, curBlockLight, lightType);
            var curSavedLightValue = GetSavedLightValueByType(cx, cy, cz, lightType);
            
            #if UNITY_EDITOR
            
            if (verbose) {
                GameObject.Destroy(GameClientDebugger.Instance.CreateDebugObject(
                    new Vector3(cx, cy, cz) + worldBasePosition.ToUnityVector3(), Color.red,
                    $"C: {computedLightValue} S: {curSavedLightValue}"),2f);
            }
            
            #endif
            
            if (computedLightValue > curSavedLightValue)
            {
                lightUpdateQueue[queueBack++] = PackLightUpdateEvent(0, 0, 0);
            }
            else if (computedLightValue < curSavedLightValue)
            {
                // recalculate the light that emitted from update point.
                lightUpdateQueue[queueBack++] = PackLightUpdateEvent(0, 0, 0, curSavedLightValue);

                while (queueHead < queueBack)
                {
                    var packedUpdateEvent = lightUpdateQueue[queueHead++];
                    UnpackLightUpdateEvent(packedUpdateEvent, out int dx, out int dy, out int dz, out int prevLightValue);

                    int nx = cx + dx, ny = cy + dy, nz = cz + dz;

                    curSavedLightValue = GetSavedLightValueByType(nx, ny, nz, lightType);

                    if (curSavedLightValue == prevLightValue)  // it might be the light emitted from update point.
                    {
                        SetLightValueByType(nx, ny, nz, lightType, 0);

                        if (prevLightValue > 0)
                        {
                            int absDx = dx > 0 ? dx : -dx;
                            int absDy = dy > 0 ? dy : -dy;
                            int absDz = dz > 0 ? dz : -dz;

                            if (absDx + absDy + absDz < 17)
                            {
                                for (int i = 0; i < 6; ++i)
                                {
                                    var sign = (i % 2) * 2 - 1; // 1 -1 1 -1...
                                    var nextDx = dx + (((i / 2) % 3) / 2) * sign;
                                    var nextDy = dy + (((i / 2 + 1) % 3) / 2) * sign;
                                    var nextDz = dz + (((i / 2 + 2) % 3) / 2) * sign;
                                    var neighbourBlockSavedLightValue =
                                        GetSavedLightValueByType(cx + nextDx, cy + nextDy, cz + nextDz, lightType);
                                    blockCode = GetBlock(cx + nextDx, cy + nextDy, cz + nextDz, 
                                        true, false).blockCode;
                                    var neighbourBlockOpacity = BlockMetaDatabase.GetBlockOpacityByCode(blockCode);

                                    if (neighbourBlockOpacity == 0)
                                    {
                                        neighbourBlockOpacity = 1;
                                    }

                                    var lightMightBe = prevLightValue - neighbourBlockOpacity;
                                    if (lightMightBe < 0) lightMightBe = 0;

                                    if (neighbourBlockSavedLightValue == lightMightBe &&
                                        queueBack + 1 < lightUpdateQueue.Length)
                                    {
                                        afterReCalcUpdate = true;
                                        
                                        lightUpdateQueue[queueBack++] = PackLightUpdateEvent(
                                            nextDx, nextDy, nextDz, neighbourBlockSavedLightValue);
                                    }
                                }
                            }
                        }
                    }
                }

                queueHead = 0;
            }
            
            Profiler.BeginSample("BFS Queue Process");
            
            while (queueHead < queueBack)
            {
                var packedUpdateEvent = lightUpdateQueue[queueHead++];
                
                UnpackLightUpdateEvent(packedUpdateEvent, out int dx, out int dy, out int dz, out int lightValue);

                int nx = cx + dx, ny = cy + dy, nz = cz + dz;

                blockCode = GetBlock(cx + dx, cy + dy, cz + dz, true, false).blockCode;
                curBlockOpacity = BlockMetaDatabase.GetBlockOpacityByCode(blockCode);
                curBlockLight = BlockMetaDatabase.GetBlockLightByCode(blockCode);
                
                curSavedLightValue = GetSavedLightValueByType(nx, ny, nz, lightType);

                if (curBlockOpacity == 0)
                    curBlockOpacity = 1;
                
                computedLightValue = ComputeAndGetBlockLightNearBy(
                    nx, ny, nz, curBlockOpacity, curBlockLight, lightType,
                    out int lightRight, out int lightLeft,
                    out int lightAbove, out int lightBelow,
                    out int lightAhead, out int lightBehind);

                if (computedLightValue == curSavedLightValue)
                {
                    if (afterReCalcUpdate)
                    {
                        if (nz >= 15)
                            if (nx >= 15) lightUpdateSurroundingBits |= 32;
                            else if (nx <= 0) lightUpdateSurroundingBits |= 128;
                            else lightUpdateSurroundingBits |= 1;
                        else if (nz <= 0)
                            if (nx >= 15) lightUpdateSurroundingBits |= 16;
                            else if (nx <= 0) lightUpdateSurroundingBits |= 64;
                            else lightUpdateSurroundingBits |= 2;
                        else if (nx >= 15) lightUpdateSurroundingBits |= 4;
                        else if (nx <= 0) lightUpdateSurroundingBits |= 8;

                    }
                    continue; // don't need to spread light
                }

                // 设置越区块检测位，用于表示这次更新是否超越了本区块的范围
                if (nz >= 15)
                    if (nx >= 15) lightUpdateSurroundingBits |= 32;
                    else if (nx <= 0) lightUpdateSurroundingBits |= 128;
                    else lightUpdateSurroundingBits |= 1;
                else if (nz <= 0)
                    if (nx >= 15) lightUpdateSurroundingBits |= 16;
                    else if (nx <= 0) lightUpdateSurroundingBits |= 64;
                    else lightUpdateSurroundingBits |= 2;
                else if (nx >= 15) lightUpdateSurroundingBits |= 4;
                else if (nx <= 0) lightUpdateSurroundingBits |= 8;


                SetLightValueByType(nx, ny, nz, lightType, computedLightValue);

                if (computedLightValue < curSavedLightValue)
                    continue;
                
                #if CHK_LIGHT_DEBUG
                
                string ns = "";
                
                #endif

                int absDx = dx > 0 ? dx : -dx;
                int absDy = dy > 0 ? dy : -dy;
                int absDz = dz > 0 ? dz : -dz;

                if (absDx + absDy + absDz < 15 && queueBack + 6 < lightUpdateQueue.Length)
                {
                    if (lightRight < computedLightValue)
                    {
                        lightUpdateQueue[queueBack++] = PackLightUpdateEvent(dx + 1, dy, dz);
                        
#if CHK_LIGHT_DEBUG
                        ns += "R";
#endif
                    }

                    if (lightLeft < computedLightValue)
                    {
                        lightUpdateQueue[queueBack++] = PackLightUpdateEvent(dx - 1, dy, dz);
                    }

                    if (lightAbove < computedLightValue)
                    {
                        lightUpdateQueue[queueBack++] = PackLightUpdateEvent(dx, dy + 1, dz);
                        
#if CHK_LIGHT_DEBUG
                        ns += "T";
#endif
                    }

                    if (lightBelow < computedLightValue)
                    {
                        lightUpdateQueue[queueBack++] = PackLightUpdateEvent(dx, dy - 1, dz);
                        
#if CHK_LIGHT_DEBUG
                        ns += "D";
#endif
                    }

                    if (lightAhead < computedLightValue)
                    {
                        lightUpdateQueue[queueBack++] = PackLightUpdateEvent(dx, dy, dz + 1);
                        
#if CHK_LIGHT_DEBUG
                        ns += "A";
#endif
                    }

                    if (lightBehind < computedLightValue)
                    {
                        lightUpdateQueue[queueBack++] = PackLightUpdateEvent(dx, dy, dz - 1);

#if CHK_LIGHT_DEBUG
                        ns += "B";
#endif
                    }
                }
                
#if CHK_LIGHT_DEBUG
                GameClientDebugger.Instance.CreateDebugObject(
                    new Vector3(nx, ny, nz) + worldBasePosition.ToUnityVector3(), Color.red,
                    $"L: {computedLightValue} QB: {queueBack} P: {cx} {cy} {cz} D: {dx} {dy} {dz}" + 
                    $" NP: {nx} {ny} {nz} NS: {ns}");
#endif
                
                #if CHK_LIGHT_DEBUG
                #endif
            }
            
            Profiler.EndSample();
            
            Profiler.EndSample();
        }

        protected void RelightBlocksInGap(int cx, int cy, int cz)
        {
            // invoked before the height map updated. 
            int height = GetHeight(cx, cz);
            int newHeight = height;

            if (cy > height) 
                newHeight = cy;

            for (; newHeight > 0 && !GetBlock(cx, newHeight, cz).IsSolid(); --newHeight) { }

            if (height == newHeight)
            {
                return;
            }

            int idx = Calc2DMapIndex(cx, cz);

            if (height > newHeight)
            {
                dirtyColumnRange[idx] = ((height& 0xff) << 8)  | (newHeight & 0xff);
            }
            else
            {
                dirtyColumnRange[idx] = ((newHeight & 0xff) << 8) | (height & 0xff);
            }
            
            cleanSkyLightColumn[idx] = false;
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

        public void OnPreRender()
        {
            Profiler.BeginSample("Chunk Pre Render");
            
            if (lightingState < 1)
            {
                // TODO: consider it.
                BakeHeightMapWithSkyLightUpdate();
                lightingState = 1;
            }
            else
            {
                BakeHeightMap();
            }
            
            if (lightingState < 2)
            {
                UpdateChunkLightMapFullyNew();
            } else if (lightingState < 3)
            {
                CheckAndUpdateSkyLights(true);
            }
            
            Profiler.EndSample();
        }
    }
}