using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Behavior;
using Block2nd.Behavior.Block;
using Block2nd.Client;
using Block2nd.Render;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.DebugUtil;
using Block2nd.GamePlay;
using Block2nd.GameSave;
using Block2nd.MathUtil;
using Block2nd.Persistence.KNBT;
using Block2nd.Phys;
using Block2nd.Scriptable;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace Block2nd.World
{
    public class Level : MonoBehaviour
    {
        public string levelFolderName = "Level_01";
        public string levelName = "Level_01";
        
        public System.Random random;

        private ChunkManager chunkManager;
        private TerrainNoiseGenerator terrainNoise;
        
        private Queue<ChunkUpdateContext> chunkUpdateQueue = new Queue<ChunkUpdateContext>();

        public WorldSettings worldSettings;

        public GameObject blockParticlePrefab;
        public GameObject chunkPrefab;

        public Player Player => client.player;
        public TerrainNoiseGenerator TerrainNoiseGenerator => terrainNoise;

        private readonly Queue<Chunk> dirtyChunkQueue = new Queue<Chunk>();

        public bool breakChunkRender;
        private bool chunkProvideIgnoreDistance;

        private GameClient client;

        private int levelTickCount = 0;
        private float tickInterval = 0.3f;
        private float lastTickTime = -10f;
        private int maxEachUpdateCount = 50;

        private bool chunkProvideLock;
        private bool chunkRenderLock;
        private bool syncProvideAndRenderLock;
        private Vector3 lastChunkProvidePosition = Vector3.positiveInfinity;
        
        public Vector3 gravity = new Vector3(0, -0.98f, 0);

        public IChunkProvider chunkProvider;

        private ChunkRenderEntityManager chunkRenderEntityManager;

        [HideInInspector] public LevelSaveHandler levelSaveHandler;
        [HideInInspector] public int seed;

        public ChunkManager ChunkManager
        {
            get
            {
                return chunkManager;
            }
        }

        private void Awake()
        {
            client = GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>();
            chunkManager = new ChunkManager(this, chunkPrefab, transform, worldSettings, client);
            terrainNoise = new TerrainNoiseGenerator(worldSettings);

            chunkProvider = new ChunkProviderGenerateOrLoad(
                new LocalChunkLoader(), 
                new EarthChunkGenerator(worldSettings));

            chunkRenderEntityManager = GetComponent<ChunkRenderEntityManager>();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void LateUpdate()
        {
            if (Time.time - lastTickTime > tickInterval)
            {
                var nUpdate = PerformChunkUpdate();
                lastTickTime = Time.time;
                client.guiCanvasManager.SetUpdateText(nUpdate);
            }

            if (dirtyChunkQueue.Count > 0)
            {
                chunkRenderEntityManager.RenderChunk(dirtyChunkQueue.Dequeue());
            }
        }

        public void SetChunkProvider(IChunkProvider chunkProvider)
        {
            this.chunkProvider = chunkProvider;
        }

        public void PrepareLevel()
        {
            GeneratePyramids();
        }

        public void CreateSaveHandler()
        {
            levelSaveHandler = new LevelSaveHandler(this);
        }

        public int PerformChunkUpdate()
        {
            if (chunkUpdateQueue.Count <= 0)
                return 0;
            
            var length = 0;

            var ctxArray = new ChunkUpdateContext[maxEachUpdateCount];
            for (; chunkUpdateQueue.Count > 0 && length < maxEachUpdateCount; ++length)
                ctxArray[length] = chunkUpdateQueue.Dequeue();
            
            var player = client.player;
            
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

            return length;
        }

        public void AddUpdateToNextTick(ChunkUpdateContext ctx)
        {
            chunkUpdateQueue.Enqueue(ctx);
        }

        public IEnumerator LevelTickCoroutine()
        {
            while (true)
            {
                LevelTick();
                levelTickCount++;

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void LevelTick()
        {
            var playerPos = client.player.transform.position;

            #if !UNITY_EDITOR 
            if (levelTickCount % 3 == 0)
            {
                chunkProvider.SaveChunk(this, false);
            }
            #endif
            
            ProvideChunksSurrounding(playerPos);
            
            client.guiCanvasManager.chunkStatText.SetChunksInCache(chunkProvider.GetChunkCacheCount());
            chunkRenderEntityManager.Tick();
        }

        public void ProvideChunksSurrounding(
                        Vector3 position, int radius = 0, bool renderImmediately = true, 
                        bool waitForProviding = false)
        {
            if (!waitForProviding && renderImmediately && !chunkProvideLock && !syncProvideAndRenderLock)
            {
                if (!chunkProvideIgnoreDistance)
                {
                    if ((lastChunkProvidePosition - position).magnitude < 48 /* 3 chunks far*/)
                    {
                        return;
                    }
                }
                else
                {
                    chunkProvideIgnoreDistance = false;
                }

                lastChunkProvidePosition = client.player.transform.position;

                StartCoroutine(SyncChunkProvideAndRenderCoroutine(position, radius));
                return;
            }

            var coroutine = ProvideChunksSurroundingCoroutine(position, radius);
                
            if (waitForProviding)
            {
                while (coroutine.MoveNext()) ;
            }
            else
            {
                if (!chunkProvideLock)
                    StartCoroutine(coroutine);
            }
            
            if (renderImmediately && !chunkRenderLock)
                StartCoroutine(RenderChunksSurrounding(position, radius));
        }

        public IEnumerator SyncChunkProvideAndRenderCoroutine(Vector3 position, int radius)
        {
            syncProvideAndRenderLock = true;
            
            yield return StartCoroutine(ProvideChunksSurroundingCoroutine(position, radius));
            yield return StartCoroutine(RenderChunksSurrounding(position, radius));
            
            syncProvideAndRenderLock = false;
        }

        public IEnumerator ProvideChunksSurroundingCoroutine(
                        Vector3 position, int radius)
        {
            if (radius == 0)
            {
                radius = client.gameSettings.viewDistance + 2;
            }
            
            chunkProvideLock = true;
            
            int pointChunkX = (int) position.x >> 4;
            int pointChunkZ = (int) position.z >> 4;
            
            for (int cx = pointChunkX - radius; cx <= pointChunkX + radius; ++cx)
            {
                for (int cz = pointChunkZ - radius; cz <= pointChunkZ + radius; ++cz)
                {
                    chunkProvider.ProvideChunk(this, cx, cz);

                    yield return null;
                }
            }

            chunkProvideLock = false;
        }
        
        // Only for the player first time enter the world.
        public IEnumerator ProvideChunksSurroundingCoroutineWithReport(Vector3 position, int radius)
        {
            var progressUI = client.guiCanvasManager.worldGeneratingProgressUI;
			
            progressUI.SetTitle("Building terrain...");
            progressUI.SetProgress(0);
            yield return null;

            if (radius == 0)
            {
                radius = client.gameSettings.viewDistance + 2;
            }
            
            chunkProvideLock = true;
            
            int pointChunkX = (int) position.x >> 4;
            int pointChunkZ = (int) position.z >> 4;

            int total = 4 * radius * radius + 4 * radius + 1;
            float count = 0;

            for (int cx = pointChunkX - radius; cx <= pointChunkX + radius; ++cx)
            {
                for (int cz = pointChunkZ - radius; cz <= pointChunkZ + radius; ++cz)
                {
                    chunkProvider.ProvideChunk(this, cx, cz);
                    count++;
                }
                
                progressUI.SetProgress(count / total);
                yield return null;
            }

            chunkProvideLock = false;
        }

        public IEnumerator RenderChunksSurrounding(Vector3 position, int distance)
        {
            chunkRenderLock = true;
            if (distance == 0)
                distance = client.gameSettings.viewDistance;

            var startChunkX = Mathf.FloorToInt(position.x) >> 4;
            var startChunkZ = Mathf.FloorToInt(position.z) >> 4;

            int cx = startChunkX, cz = startChunkZ;
            
            chunkRenderEntityManager.TryRenderChunk(chunkProvider.TryGetChunk(this, cx, cz));
            yield return null;

            for (int i = 1; i <= distance; ++i)
            {
                // dir Z+
                cz = startChunkZ + i;
                cx = startChunkX;
                chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                yield return null;
                for (int j = 1; j <= i; ++j)
                {
                    cx = startChunkX + j;
                    chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                    yield return null;
                    cx = startChunkX - j;
                    chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                    yield return null;
                }
                
                // dir Z-
                cz = startChunkZ - i;
                cx = startChunkX;
                chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                yield return null;
                for (int j = 1; j <= i; ++j)
                {
                    cx = startChunkX + j;
                    chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                    yield return null;
                    cx = startChunkX - j;
                    chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                    yield return null;
                }
                
                // dir X+
                cz = startChunkZ;
                cx = startChunkX + i;
                chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                yield return null;
                for (int j = 1; j <= i; ++j)
                {
                    cz = startChunkZ + j;
                    chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                    yield return null;
                    cz = startChunkZ - j;
                    chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                    yield return null;
                }
                
                // dir X-
                cz = startChunkZ;
                cx = startChunkX - i;
                chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                yield return null;
                for (int j = 1; j <= i; ++j)
                {
                    cz = startChunkZ + j;
                    chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                    yield return null;
                    cz = startChunkZ - j;
                    chunkRenderEntityManager.TryRenderChunk(chunkProvider.ProvideChunk(this, cx, cz));
                    yield return null;
                }

                if (breakChunkRender)
                {
                    breakChunkRender = false;
                    chunkProvideIgnoreDistance = true;
                    break;
                }
            }
            
            chunkRenderLock = false;
        }

        public bool IsChunkSurroundingInCache(int chunkX, int chunkZ)
        {
            return chunkProvider.GetChunkInCache(this, chunkX - 1, chunkZ) != null &&
                   chunkProvider.GetChunkInCache(this, chunkX + 1, chunkZ) != null &&
                   chunkProvider.GetChunkInCache(this, chunkX, chunkZ + 1) != null &&
                   chunkProvider.GetChunkInCache(this, chunkX, chunkZ - 1) != null;
        }
        
        private bool IsBoundsInFrustum(Bounds aabb)
        {
            Plane[] planes = new Plane[6];
            GeometryUtility.CalculateFrustumPlanes(client.player.playerCamera, planes);
            return GeometryUtility.TestPlanesAABB(planes, aabb);
        }

        public List<IntVector3> GetAllChunkCoordsInFrustum()
        {
            var chunkHeight = worldSettings.chunkHeight;
            
            var viewDistance = client.gameSettings.viewDistance;
            var playerCamera = client.player.playerCamera;
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

        public void ResetChunkProvider(IChunkProvider chunkProvider)
        {
            this.chunkProvider = chunkProvider;
        }
        
        private void GeneratePyramids()
        {
            var x = 500;
            var z = 500;

            var y = chunkProvider.GetChunkGenerator().GetBaseHeight();
            
            var redBrickCode = BlockMetaDatabase.GetBlockMetaById("b2nd:block/red_brick").blockCode;

            for (int h = 0; h < 64; ++h)
            {
                for (int px = -64 + h; px < 64 - h; ++px)
                {
                    for (int pz = -64 + h; pz < 64 - h; ++pz)
                    {
                        SetBlock(redBrickCode, x + px, y + h, z + pz, false, false);
                    }
                }
            }
        }

        public void GrowTree(int worldX, int worldY, bool check = true)
        {
            var waterCode = BlockMetaDatabase.GetBlockCodeById("b2nd:block/water");

            chunkManager.BakeAllChunkHeightMap();

            var x = worldX;
            var z = worldY;

            var chunk = GetChunkFromCoords(worldX >> 4, worldY >> 4);
            chunk.BakeHeightMap();
            
            var chunkLocalPos = chunk.WorldToLocal(x, z);

            var y = chunk.heightMap[chunkLocalPos.x, chunkLocalPos.z];
            var baseBlock = GetBlock(x, y, z);

            if (check && baseBlock.blockCode != BlockMetaDatabase.BuiltinBlockCode.Grass)
                return;

            var orkCode = BlockMetaDatabase.GetBlockMetaById("b2nd:block/ork").blockCode;
            var leavesCode = BlockMetaDatabase.GetBlockMetaById("b2nd:block/leaves").blockCode;

            SetBlockFast(orkCode, x, ++y, z);
            SetBlockFast(orkCode, x, ++y, z);
            SetBlockFast(orkCode, x, ++y, z);

            SetBlockFast(leavesCode, x - 1, y, z - 1);
            SetBlockFast(leavesCode, x, y, z - 1);
            SetBlockFast(leavesCode, x + 1, y, z - 1);
            SetBlockFast(leavesCode, x - 1, y, z);
            SetBlockFast(leavesCode, x + 1, y, z);
            SetBlockFast(leavesCode, x - 1, y, z + 1);
            SetBlockFast(leavesCode, x, y, z + 1);
            SetBlockFast(leavesCode, x + 1, y, z + 1);

            SetBlockFast(leavesCode, x + 2, y, z - 1);
            SetBlockFast(leavesCode, x + 2, y, z);
            SetBlockFast(leavesCode, x + 2, y, z + 1);

            SetBlockFast(leavesCode, x - 2, y, z + 1);
            SetBlockFast(leavesCode, x - 2, y, z);
            SetBlockFast(leavesCode, x - 2, y, z - 1);

            SetBlockFast(leavesCode, x - 1, y, z + 2);
            SetBlockFast(leavesCode, x, y, z + 2);
            SetBlockFast(leavesCode, x + 1, y, z + 2);

            SetBlockFast(leavesCode, x - 1, y, z - 2);
            SetBlockFast(leavesCode, x, y, z - 2);
            SetBlockFast(leavesCode, x + 1, y, z - 2);

            SetBlockFast(orkCode, x, ++y, z);

            SetBlockFast(leavesCode, x, y, z + 1);
            SetBlockFast(leavesCode, x, y, z - 1);
            SetBlockFast(leavesCode, x + 1, y, z);
            SetBlockFast(leavesCode, x - 1, y, z);

            SetBlockFast(leavesCode, x + 2, y, z - 1);
            SetBlockFast(leavesCode, x + 2, y, z);
            SetBlockFast(leavesCode, x + 2, y, z + 1);

            SetBlockFast(leavesCode, x - 2, y, z + 1);
            SetBlockFast(leavesCode, x - 2, y, z);
            SetBlockFast(leavesCode, x - 2, y, z - 1);

            SetBlockFast(leavesCode, x - 1, y, z + 2);
            SetBlockFast(leavesCode, x, y, z + 2);
            SetBlockFast(leavesCode, x + 1, y, z + 2);

            SetBlockFast(leavesCode, x - 1, y, z - 2);
            SetBlockFast(leavesCode, x, y, z - 2);
            SetBlockFast(leavesCode, x + 1, y, z - 2);

            SetBlockFast(orkCode, x, ++y, z);

            SetBlockFast(leavesCode, x, y, z + 1);
            SetBlockFast(leavesCode, x, y, z - 1);
            SetBlockFast(leavesCode, x + 1, y, z);
            SetBlockFast(leavesCode, x - 1, y, z);
            SetBlockFast(leavesCode, x, y, z);

            SetBlockFast(orkCode, x, ++y, z);

            SetBlockFast(leavesCode, x, y, z + 1);
            SetBlockFast(leavesCode, x, y, z - 1);
            SetBlockFast(leavesCode, x + 1, y, z);
            SetBlockFast(leavesCode, x - 1, y, z);
            SetBlockFast(leavesCode, x, y, z);
        }

        public void Explode(int x, int y, int z, int radius)
        {
            Profiler.BeginSample("Explode");
            
            var minX = x - radius;
            var minY = y - radius;
            var minZ = z - radius;

            int radiusSqure = radius * radius;

            for (int cx = minX; cx <= x + radius; ++cx)
            {
                for (int cy = minY; cy <= y + radius; ++cy)
                {
                    for (int cz = minZ; cz <= z + radius; ++cz)
                    {
                        var dirSquare = Mathf.Pow(cx - x, 2) + Mathf.Pow(cy - y, 2) + Mathf.Pow(cz - z, 2);
                        if (dirSquare <= radiusSqure)
                        {
                            Chunk cp;
                            var block = GetBlock(cx, cy, cz, out cp);
                            var defaultAct = true;
                            
                            if (cx != x && cy != y && cz != z)
                            {
                                defaultAct = BlockMetaDatabase.GetBlockBehaviorByCode(block.blockCode).OnHurt(
                                    new IntVector3(cx, cy, cz), this, cp, client.player);
                            }

                            if (defaultAct)
                            {
                                SetBlock(0, cx, cy, cz, notify: false);
                            }
                        }
                    }
                }
            }

            var dir = client.player.transform.position - new Vector3(x, y, z);
            client.player.playerController.AddImpulseForse(dir.normalized * 300 / (1f + dir.magnitude));
            
            Profiler.EndSample();
        }

        public int GetSkyLight(int x, int y, int z, bool cacheOnly = false)
        {
            Chunk chunk;
            if (cacheOnly)
            {
                chunk = chunkProvider.GetChunkInCache(this, x >> 4, z >> 4);
            }
            else
            {
                chunk = GetChunkFromCoords(x >> 4, z >> 4);
            }

            var chunkLocalPos = chunk.WorldToLocal(x, z);

            if (y < 0 || y >= worldSettings.chunkHeight)
                return 0;
            
            return chunk.lightMap[chunkLocalPos.x, y, chunkLocalPos.z];
        }

        public int GetHeight(int x, int z, bool cacheOnly = false)
        {
            Chunk chunk;
            if (cacheOnly)
            {
                chunk = chunkProvider.GetChunkInCache(this, x >> 4, z >> 4);
                if (chunk == null)
                    return 0;
            }
            else
            {
                chunk = GetChunkFromCoords(x >> 4, z >> 4);
            }

            var chunkLocalPos = chunk.WorldToLocal(x, z);
            
            if (chunk.dirty)
                chunk.BakeHeightMap();
            
            return chunk.heightMap[chunkLocalPos.x, chunkLocalPos.z];
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

        public ChunkBlockData GetBlock(Vector3 pos, 
                                        bool autoGenerate = true, bool cacheOnly = false)
        {
            return GetBlock((int) pos.x, (int) pos.y, (int) pos.z, out Chunk _, autoGenerate, cacheOnly);
        }
        
        public ChunkBlockData GetBlock(int x, int y, int z, 
                                        bool autoGenerate = true, bool cacheOnly = false)
        {
            return GetBlock(x, y, z, out Chunk _, autoGenerate, cacheOnly);
        }

        public ChunkBlockData GetBlock(int x, int y, int z, out Chunk locatedChunk, 
                                        bool autoGenerate = true, bool cacheOnly = false)
        {
            int chunkX = x >> 4;
            int chunkZ = z >> 4;

            Chunk chunk;

            if (cacheOnly)
            {
                chunk = chunkProvider.GetChunkInCache(this, chunkX, chunkZ);
                locatedChunk = chunk;
                if (chunk == null)
                {
                    return ChunkBlockData.EMPTY;
                }
            }
            else if (autoGenerate)
            {
                chunk = chunkProvider.ProvideChunk(this, chunkX, chunkZ);
            }
            else
            {
                chunk = chunkProvider.TryGetChunk(this, chunkX, chunkZ);
                locatedChunk = chunk;
                if (chunk == null)
                    return ChunkBlockData.EMPTY;
            }

            var local = chunk.WorldToLocal(x, y, z);

            locatedChunk = chunk;
            
            if (y < 0 || y >= worldSettings.chunkHeight || local.x < 0 || local.x >= 16 || local.z < 0 || local.z >= 16)
                return ChunkBlockData.EMPTY;

            return chunk.chunkBlocks[local.x, local.y, local.z];
        }

        public Chunk GetChunkFromCoords(int chunkX, int chunkZ, bool cacheOnly = false)
        {
            if (cacheOnly)
                return chunkProvider.GetChunkInCache(this, chunkX, chunkZ);
            return chunkProvider.ProvideChunk(this, chunkX, chunkZ);
        }

        public void SetBlockState(int x, int y, int z, byte state, bool updateMesh)
        {
            int chunkX = x >> 4;
            int chunkZ = z >> 4;

            var chunk = chunkProvider.ProvideChunk(this, chunkX, chunkZ);
            var local = chunk.WorldToLocal(x, y, z);

            chunk.chunkBlocks[local.x, local.y, local.z].blockState = state;

            chunk.modified = true;

            if (updateMesh)
            {
                chunkRenderEntityManager.RenderChunk(chunk);
            }
        }

        public void SetBlockFast(int blockCode, int x, int y, int z, bool useInitState = true, byte state = 0)
        {
            int chunkX = x >> 4;
            int chunkZ = z >> 4;

            if (y >= worldSettings.chunkHeight || y < 0)
                return;

            var chunk = chunkProvider.ProvideChunk(this, chunkX, chunkZ);
            var local = chunk.WorldToLocal(x, y, z);

            var meta = BlockMetaDatabase.GetBlockMetaByCode(blockCode);

            if (useInitState && blockCode > 0)
                state = meta.initState;
            
            chunk.chunkBlocks[local.x, local.y, local.z] = new ChunkBlockData
            {
                blockCode = blockCode,
                blockState = state
            };

            chunk.modified = true;
            chunk.dirty = true;
        }
        
        public void SetBlock(int blockCode, int x, int y, int z, 
                                bool notify = false, bool useInitState = true, byte state = 0)
        {
            int chunkX = x >> 4;
            int chunkZ = z >> 4;

            if (y >= worldSettings.chunkHeight || y < 0)
                return;

            var chunk = chunkProvider.ProvideChunk(this, chunkX, chunkZ);
            var local = chunk.WorldToLocal(x, y, z);

            var meta = BlockMetaDatabase.GetBlockMetaByCode(blockCode);

            if (useInitState && blockCode > 0)
                state = meta.initState;
            
            chunk.chunkBlocks[local.x, local.y, local.z] = new ChunkBlockData
            {
                blockCode = blockCode,
                blockState = state
            };
            
            if (blockCode > 0)
                meta.behavior.OnInit(new IntVector3(x, y, z), this, chunk, client.player);

            chunk.dirty = true;
            chunk.modified = true;

            if (notify)
            {
                AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = chunk,
                    pos = new IntVector3(x, y, z)
                });
            }

            if (!dirtyChunkQueue.Contains(chunk))
                dirtyChunkQueue.Enqueue(chunk);
        }

        public RayHit RaycastBlocks(Vector3 start, Vector3 end)
        {
            start += new Vector3(5e-5f, 5e-5f, -5e-5f);
            int iStartX = Mathf.FloorToInt(start.x);
            int iStartY = Mathf.FloorToInt(start.y);
            int iStartZ = Mathf.FloorToInt(start.z);
            int iEndX = Mathf.FloorToInt(end.x);
            int iEndY = Mathf.FloorToInt(end.y);
            int iEndZ = Mathf.FloorToInt(end.z);

            RayHit hit;
            ChunkBlockData block = GetBlock(iStartX, iStartY, iStartZ);
            var blockBehavior = BlockMetaDatabase.GetBlockBehaviorByCode(block.blockCode);
            
            if (blockBehavior.CanRaycast() &&
                (hit = blockBehavior.GetAABB(iStartX, iStartY, iStartZ).Raycast(start, end)) != null)
            {
                return hit;
            }

            byte normalDirection = 255;

            int blockTraceCount = 100;

            while (blockTraceCount-- > 0)
            {
                if (Single.IsNaN(start.x) || Single.IsNaN(start.y) || Single.IsNaN(start.z))
                    return null;
                
                float newX = 1000f, newY = 1000f, newZ = 1000f;
                bool xMoved = true, yMoved = true, zMoved = true;

                if (iStartX == iEndX && iStartY == iEndY && iStartZ == iEndZ)
                {
                    return null;
                }
                
                if (iEndX > iStartX)
                {
                    newX = iStartX + 1;
                }
                else if (iEndX < iStartX)
                {
                    newX = iStartX;
                }
                else
                {
                    xMoved = false;
                }

                if (iEndY > iStartY)
                {
                    newY = iStartY + 1;
                }
                else if (iEndY < iStartY)
                {
                    newY = iStartY;
                }
                else
                {
                    yMoved = false;
                }

                if (iEndZ > iStartZ)
                {
                    newZ = iStartZ + 1;
                }
                else if (iEndZ < iStartZ)
                {
                    newZ = iStartZ;
                }
                else
                {
                    zMoved = false;
                }
                
                float dx = end.x - start.x;
                float dy = end.y - start.y;
                float dz = end.z - start.z;
                float ratioX = xMoved ? (newX - start.x) / dx : 1000f;
                float ratioY = yMoved ? (newY - start.y) / dy : 1000f;
                float ratioZ = zMoved ? (newZ - start.z) / dz : 1000f;
                
                if (ratioX < ratioY && ratioX < ratioZ) // x direction is closest
                {
                    if (iStartX < iEndX)
                    {
                        normalDirection = RayHitNormalDirection.Left;
                    }
                    else
                    {
                        normalDirection = RayHitNormalDirection.Right;
                    }

                    start.x = newX;
                    start.y += dy * ratioX;
                    start.z += dz * ratioX;
                }
                else if (ratioY < ratioZ) // y direction is cloest.
                {
                    if (iStartY < iEndY)
                    {
                        normalDirection = RayHitNormalDirection.Down;
                    }
                    else
                    {
                        normalDirection = RayHitNormalDirection.Up;
                    }

                    start.y = newY;
                    start.x += dx * ratioY;
                    start.z += dz * ratioY;
                }
                else
                {
                    if (iStartZ < iEndZ)
                    {
                        normalDirection = RayHitNormalDirection.Back;
                    }
                    else
                    {
                        normalDirection = RayHitNormalDirection.Forward;
                    }

                    start.y += dy * ratioZ;
                    start.x += dx * ratioZ;
                    start.z = newZ;
                }

                // reset new int start xyz.
                iStartX = Mathf.FloorToInt(start.x);
                iStartY = Mathf.FloorToInt(start.y);
                iStartZ = Mathf.FloorToInt(start.z);

                if (normalDirection == RayHitNormalDirection.Right)
                    iStartX--;
                
                if (normalDirection == RayHitNormalDirection.Up) // if top face, offset it.
                    iStartY--;
                
                if (normalDirection == RayHitNormalDirection.Forward)  // if front face, offset it.
                    iStartZ--;

                blockBehavior = BlockMetaDatabase.GetBlockBehaviorByCode(
                    GetBlock(iStartX, iStartY, iStartZ).blockCode);
                var aabb = blockBehavior.GetAABB(iStartX, iStartY, iStartZ);
                
                if (blockBehavior.CanRaycast() && 
                    (hit = aabb.Raycast(start, end)) != null)
                {
                    return hit;
                }
            }

            return null;
        }

        public List<AABB> GetWorldCollideBoxIntersect(AABB aabb)
        {
            var x0 = Mathf.FloorToInt(aabb.minX);
            var x1 = Mathf.FloorToInt(aabb.maxX) + 1;
            var y0 = Mathf.FloorToInt(aabb.minY);
            var y1 = Mathf.FloorToInt(aabb.maxY) + 1;
            var z0 = Mathf.FloorToInt(aabb.minZ);
            var z1 = Mathf.FloorToInt(aabb.maxZ) + 1;

            var result = new List<AABB>();

            for (int x = x0; x < x1; ++x)
            {
                for (int y = y0; y < y1; ++y)
                {
                    for (int z = z0; z < z1; ++z)
                    {
                        var block = GetBlock(x, y, z);
                        AABB blockBox = BlockMetaDatabase.GetBlockBehaviorByCode(block.blockCode)
                                            .GetAABB(new IntVector3(x, y, z));

                        if (block.blockCode != 0 &&  
                            !BlockMetaDatabase.GetBlockMetaByCode(block.blockCode).liquid &&
                            aabb.Intersects(blockBox))
                        {
                            result.Add(blockBox);
                        }
                    }
                }
            }
            
            // Debug.Log(result.Count + ", " + aabb.Center);

            return result;
        } 

        public Vector3 CalculateWorldBlockPosByHit(RaycastHit hit)
        {
            var point = new Vector3((int) hit.point.x, (int) hit.point.y, (int) hit.point.z);
            var normal = hit.normal.normalized;

            if (normal == Vector3.up)
            {
                if (Mathf.Abs(hit.point.y - point.y) < 0.1)
                    point.y -= 1;
            }
            else if (normal == Vector3.right)
            {
                if (Mathf.Abs(hit.point.x - point.x) < 0.1)
                    point.x -= 1;
            }
            else if (normal == Vector3.forward)
            {
                if (Mathf.Abs(hit.point.z - point.z) < 0.1)
                    point.z -= 1;
            }

            return point;
        }

        public void CreateBlockParticle(Vector3 basePos)
        {
            var meta = BlockMetaDatabase.GetBlockMetaByCode(GetBlock(basePos).blockCode);
            if (meta == null)
                return;
            
            var particleGameObject = Instantiate(blockParticlePrefab);
            var pos = meta.shape.GetCenterPoint() + basePos;
            var controller = particleGameObject.GetComponent<BlockParticleController>();

            particleGameObject.transform.position = pos;
            controller.GetMaterial().SetVector("_Texcoord", 
                meta.shape.GetShapeMesh(1, 1, 0).texcoords[0]);
            
            Destroy(particleGameObject, 2f);
        }

        public void SaveLevelData()
        {
            if (levelSaveHandler == null)
                return;
            
            var levelDataWriter = levelSaveHandler.GetLevelDataWriter();
            var levelKnbt = new KNBTTagCompound("Level");

            levelKnbt.SetInt("Version", 1);
            levelKnbt.SetString("Name", levelName);
            levelKnbt.SetInt("Type", chunkProvider.GetChunkGenerator().GetId());
            levelKnbt.SetInt("Seed", worldSettings.seed);
            
            levelKnbt.Write(levelDataWriter);
            
            Debug.Log("Level: Save level data: [" + levelName + "]:[" + levelFolderName + "]");
            
            levelDataWriter.Dispose();
        }

        public void SavePlayerData()
        {
            var playerDataWriter = levelSaveHandler.GetPlayerDataWriter();
            
            var player = client.player;
            var playerKnbt = player.GetPlayerKNBTData();
            playerKnbt.Write(playerDataWriter);
            
            playerDataWriter.Dispose();
        }

        public void SaveLevelCompletely()
        {
            if (levelSaveHandler == null)
                return;

            SaveLevelData();
            SavePlayerData();
            
            chunkProvider.SaveChunk(this, true);
        }
    }
}