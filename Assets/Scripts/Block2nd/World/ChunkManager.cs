﻿using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Behavior;
using Block2nd.Client;
using Block2nd.Database;
using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Block2nd.World
{
    public class ChunkEntry
    {
        public IntVector3 basePos;
        public Chunk chunk;
    }

    public class ChunkUpdateContext
    {
        public IntVector3 pos;
        public Chunk chunk;
        public int size = 3;
        public bool onlyUpdateCenterBlock = false;
    }

    public class ChunkManager
    {
        private GameObject chunkPrefabObject;
        private Transform levelTransform;

        public List<ChunkEntry> chunkEntries = new List<ChunkEntry>();
        public Dictionary<long, Chunk> chunkDict = new Dictionary<long, Chunk>();

        public bool chunkWorkerRunning = false;

        public GameClient gameClient;

        private Level level;
        private WorldSettings worldSettings;
        private bool forceManagement = false;
        private Queue<ChunkUpdateContext> chunkUpdateQueue = new Queue<ChunkUpdateContext>();
        
        private float lastSortTime = -10;
        private float minSortInterval = 5f;
        private IntVector3 lastChunkListSortIntPos;
        private int maxEachUpdateCount = 30;

        public ChunkManager(Level Level, GameObject chunkPrefabObject, Transform levelTransform,
            WorldSettings worldSettings, GameClient gameClient)
        {
            this.chunkPrefabObject = chunkPrefabObject;
            this.levelTransform = levelTransform;
            this.worldSettings = worldSettings;
            this.gameClient = gameClient;
        }

        private static void CalcChunkGridPos(int x, int z, out int ox, out int oz)
        {
            ox = x >> 4;
            oz = z >> 4;
        }

        public void AddUpdateToNextTick(ChunkUpdateContext ctx)
        {
            chunkUpdateQueue.Enqueue(ctx);
        }

        public long ChunkCoordsToLongKey(IntVector3 pos)
        {
            long key = (long) (pos.x / 16) << 32 | (uint) (pos.z / 16);

            return key;
        }

        public long RegisterChunk(Chunk chunk)
        {
            var entry = new ChunkEntry
            {
                chunk = chunk,
                basePos = new IntVector3(chunk.worldBasePosition.x, 0, chunk.worldBasePosition.z)
            };

            var key = ChunkCoordsToLongKey(chunk.worldBasePosition);
            chunkDict.Add(key, chunk);
            chunkEntries.Add(entry);

            return key;
        }

        public Chunk FindChunk(int x, int z)
        {
            if (chunkDict.TryGetValue(ChunkCoordsToLongKey(new IntVector3(x, 0, z)), out var chunk))
            {
                return chunk;
            }

            return null;
            
            /*
            
            var chunkWidth = worldSettings.chunkWidth;

            foreach (var entry in chunkEntries)
            {
                var baseX = entry.basePos.x;
                var baseZ = entry.basePos.z;
                if (x >= baseX && x < baseX + chunkWidth && z >= baseZ && z < baseZ + chunkWidth)
                {
                    return entry.chunk;
                }
            }

            return null;
            
            */
        }

        public void SetBlock(int blockCode, int x, int y, int z, bool updateMesh = false,
            bool updateHeightmap = true, bool triggerUpdate = false, byte state = 0)
        {
            var chunk = FindChunk(x, z);
            if (chunk == null)
            {
                int cx, cz;
                CalcChunkGridPos(x, z, out cx, out cz);
                chunk = level.AllocNewChunkGameObject(cx, cz);
            }

            chunk.SetBlock(blockCode, x, y, z, true,
                updateMesh, updateHeightmap, state);

            BlockMetaDatabase.GetBlockBehaviorByCode(blockCode).OnInit(
                new IntVector3(x, y, z), gameClient.CurrentLevel, chunk, gameClient.player);

            if (triggerUpdate)
            {
                AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = chunk,
                    pos = new IntVector3(x, y, z)
                });
            }
        }

        public void SetBlockState(int x, int y, int z, byte state, bool updateMesh)
        {
            var chunk = FindChunk(x, z);
            if (chunk == null)
            {
                int cx, cz;
                CalcChunkGridPos(x, z, out cx, out cz);
                chunk = level.AllocNewChunkGameObject(cx, cz);
            }
            
            chunk.SetBlockState(x, y, z, state, true, updateMesh);
        }

        public ChunkBlockData GetBlock(int x, int y, int z)
        {
            return GetBlock(x, y, z, out Chunk _);
        }

        public ChunkBlockData GetBlock(int x, int y, int z, out Chunk locatedChunk)
        {
            var chunk = FindChunk(x, z);

            locatedChunk = chunk;

            if (chunk == null)
                return ChunkBlockData.EMPTY;

            return chunk.GetBlockWorldPos(x, y, z, false);
        }

        public void BakeAllChunkHeightMap()
        {
            foreach (var entry in chunkEntries)
            {
                entry.chunk.BakeHeightMap();
            }
        }

        public List<IntVector3> GetAllChunkCoordsInFrustum()
        {
            var chunkHeight = worldSettings.chunkHeight;
            
            var viewDistance = gameClient.gameSettings.viewDistance;
            var playerCamera = gameClient.player.playerCamera;
            var cameraPos = playerCamera.transform.position;
            var forward = playerCamera.transform.forward;
            var right = playerCamera.transform.right;

            var coords = new List<IntVector3>();

            Vector3 currentPos = cameraPos;

            Bounds aabb = new Bounds(Vector3.zero, new Vector3(16, chunkHeight, 16));

            for (int i = 0; i < viewDistance; ++i)
            {
                aabb.center = currentPos;
                if (!IsBoundsInFrustum(aabb))
                {
                    continue;
                }
                coords.Add(IntVector3.NewWithFloorToChunkGridCoord(aabb.center));

                for (;;)
                {
                    aabb.center += right;
                    if (!IsBoundsInFrustum(aabb))
                    {
                        break;
                    }
                    coords.Add(IntVector3.NewWithFloorToChunkGridCoord(aabb.center));
                }

                aabb.center = currentPos;
                for (;;)
                {
                    aabb.center -= right;
                    if (!IsBoundsInFrustum(aabb))
                    {
                        break;
                    }
                    coords.Add(IntVector3.NewWithFloorToChunkGridCoord(aabb.center));
                }
            }

            return coords;
        }
        
        public bool IsBoundsInFrustum(Bounds aabb)
        {
            Plane[] planes = new Plane[6];
            GeometryUtility.CalculateFrustumPlanes(gameClient.player.playerCamera, planes);
            return GeometryUtility.TestPlanesAABB(planes, aabb);
        }

        public void UpdateDirtyChunkCoroutine()
        {
            foreach (var entries in chunkEntries)
            {
                if (entries.chunk.dirty)
                {
                    entries.chunk.UpdateChuckMesh();
                }
            }
        }

        public IEnumerator RenderAllChunkMesh(Action callback)
        {
            var progressUI = gameClient.guiCanvasManager.worldGeneratingProgressUI;

            int count = 1;
            float totalChunkCount = chunkEntries.Count;

            float headChunkCount = totalChunkCount > 16 ? 16 : totalChunkCount;

            foreach (var entry in chunkEntries)
            {
                if (count == 16)
                {
                    progressUI.SetTitle("Ready");

                    progressUI.SetProgress("Very soon !! ");

                    yield return new WaitForSeconds(0.3f);

                    callback();
                }

                entry.chunk.UpdateChuckMesh();

                progressUI.SetProgress(count / headChunkCount);
                count++;

                yield return null;
            }

            if (totalChunkCount < 16)
            {
                callback();
            }
        }

        private int CompareChunks(Chunk a, Chunk b, IntVector3 intPoint)
        {
            var distanceA = a.worldBasePosition.PlaneDistanceSqure(intPoint);
            var distanceB = b.worldBasePosition.PlaneDistanceSqure(intPoint);

            if (distanceA > distanceB)
                return 1;

            if (distanceA < distanceB)
                return -1;

            return 0;
        }

        public void SortChunksByDistance(Vector3 point, bool reverse = false)
        {
            var intPoint = new IntVector3(point);

            /*
            if (Time.time - lastSortTime < minSortInterval &&
                lastChunkListSortIntPos.DistanceSqure(intPoint) < 9 * worldSettings.chunkWidth)
            {
                return;
            }
            */

            if (reverse)
            {
                chunkEntries.Sort((a, b) =>
                {
                    var distanceA = a.basePos.PlaneDistanceSqure(intPoint);
                    var distanceB = b.basePos.PlaneDistanceSqure(intPoint);

                    if (distanceA > distanceB)
                        return -1;

                    if (distanceA < distanceB)
                        return 1;

                    return 0;
                });
            }
            else
            {
                chunkEntries.Sort((a, b) =>
                {
                    var distanceA = a.basePos.PlaneDistanceSqure(intPoint);
                    var distanceB = b.basePos.PlaneDistanceSqure(intPoint);

                    if (distanceA > distanceB)
                        return 1;

                    if (distanceA < distanceB)
                        return -1;

                    return 0;
                });
            }

            // Debug.Log("ChunkManager: chunks sorted.");

            lastSortTime = Time.time;
            lastChunkListSortIntPos = intPoint;
        }

        public void ForceBeginChunksManagement()
        {
            forceManagement = true;
        }

        public int PerformChunkUpdate()
        {
            if (chunkUpdateQueue.Count <= 0)
                return 0;
            
            var length = 0;

            var ctxArray = new ChunkUpdateContext[maxEachUpdateCount];
            for (; chunkUpdateQueue.Count > 0 && length < maxEachUpdateCount; ++length)
                ctxArray[length] = chunkUpdateQueue.Dequeue();
            
            var player = gameClient.player;

            for (int i = 0; i < length; ++i)
            {
                var ctx = ctxArray[i];
                var pos = ctx.pos;

                pos = ctx.chunk.WorldToLocal(pos.x, pos.y, pos.z);
                
                if (ctx.onlyUpdateCenterBlock)
                    ctx.chunk.UpdateBlock(pos.x, pos.y, pos.z);
                else
                    ctx.chunk.ChunkUpdate(pos.x, pos.y, pos.z, ctx.size);
            }
            
            if (length > 0)
            {
                SortChunksByDistance(player.transform.position);
                ForceBeginChunksManagement();
            }
            
            return length;
        }

        public bool CheckIsActiveChunk(Chunk chunk, IntVector3 playerIntPos, int chunkWidth)
        {
            var chunkPos = chunk.worldBasePosition;
            chunkPos.x += chunkWidth / 2;
            chunkPos.z += chunkWidth / 2;

            var distance = Mathf.Sqrt(chunkPos.PlaneDistanceSqure(playerIntPos));

            if (distance > gameClient.gameSettings.viewDistance * chunkWidth)
            {
                return false;
            }

            return true;
        }

        public IEnumerator ChunkManagementWorkerCoroutine()
        {
            var player = gameClient.player;

            chunkWorkerRunning = true;

            var yieldTick = 0;

            while (chunkWorkerRunning)
            {
                var playerIntPos = new IntVector3(player.transform.position);

                for (int i = 0; i < chunkEntries.Count; ++i)
                {
                    if (forceManagement)
                    {
                        forceManagement = false;
                        break;
                    }

                    var entry = chunkEntries[i];

                    if (playerIntPos.PlaneDistanceSqure(lastChunkListSortIntPos) >
                        gameClient.gameSettings.viewDistance << 4)
                    {
                        SortChunksByDistance(player.transform.position);
                        break;
                    }

                    if (CheckIsActiveChunk(entry.chunk, playerIntPos, 16))
                    {
                        entry.chunk.gameObject.SetActive(true);
                        if (entry.chunk.dirty || !entry.chunk.rendered)
                            entry.chunk.UpdateChuckMesh();
                    }
                    else
                    {
                        entry.chunk.gameObject.SetActive(false);
                    }

                    if (yieldTick > 1)
                    {
                        yieldTick = 0;
                        yield return null;
                    }

                    yieldTick++;
                }
            }
        }
    }
}