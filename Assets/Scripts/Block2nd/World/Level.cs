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
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace Block2nd.World
{
    public class Level : MonoBehaviour
    {
        public string levelName = "Level_01";
        
        public System.Random random = new System.Random();

        private ChunkManager chunkManager;
        private TerrainNoiseGenerator terrainNoise;
        
        private Queue<ChunkUpdateContext> chunkUpdateQueue = new Queue<ChunkUpdateContext>();

        public WorldSettings worldSettings;

        public GameObject blockParticlePrefab;
        public GameObject chunkPrefab;

        public Player Player => client.player;
        public TerrainNoiseGenerator TerrainNoiseGenerator => terrainNoise;

        private readonly Queue<Chunk> dirtyChunkPreFrameQueue = new Queue<Chunk>();

        public bool breakChunkRender;
        private bool chunkProvideIgnoreDistance;

        private GameClient client;

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
            
            CreateSaveHandler();
        }

        private void Update()
        {
            
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void LateUpdate()
        {
            dirtyChunkPreFrameQueue.Clear();
            
            if (Time.time - lastTickTime > tickInterval)
            {
                var nUpdate = PerformChunkUpdate();
                lastTickTime = Time.time;
                client.guiCanvasManager.SetUpdateText(nUpdate);
            }
        }

        private void FlushDirtyChunkQueue(bool discard = false)
        {
            var n = dirtyChunkPreFrameQueue.Count;
            
            for (int i = 0; i < n; ++i)
                if (!discard)
                    chunkRenderEntityManager.TryRenderChunk(dirtyChunkPreFrameQueue.Dequeue());
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
            
            FlushDirtyChunkQueue();

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

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void LevelTick()
        {
            var playerPos = client.player.transform.position;
            
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
                radius = client.gameSettings.viewDistance;
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
                radius = client.gameSettings.viewDistance;
            }
            
            chunkProvideLock = true;
            
            int pointChunkX = (int) position.x >> 4;
            int pointChunkZ = (int) position.z >> 4;

            for (int cx = pointChunkX - radius; cx <= pointChunkX + radius; ++cx)
            {
                for (int cz = pointChunkZ - radius; cz <= pointChunkZ + radius; ++cz)
                {
                    chunkProvider.ProvideChunk(this, cx, cz);
                }
                
                progressUI.SetProgress((cx + radius) / (2f * radius));
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
        
        /*
        public IEnumerator CreateLevelCoroutine(TerrainNoiseGenerator noiseGenerator = null)
        {
            if (noiseGenerator != null)
            {
                this.terrainNoise = noiseGenerator;
            }
            
            Resources.UnloadUnusedAssets();
            
            var progressUI = client.guiCanvasManager.worldGeneratingProgressUI;
            
            var levelWidth = worldSettings.levelWidth;
            
            int x = levelWidth / 2;
            int z = levelWidth / 2;
            
            progressUI.gameObject.SetActive(true);
            
            progressUI.SetTitle("Generating terrainNoise...");

            yield return null;
            
            // yield return GenerateLevelBlocksCoroutine();
            
            progressUI.SetTitle("Making rivers...");
            
            if (!(terrainNoise is FlatTerrainNoiseGenerator) && !(terrainNoise is HonkaiTerrainNoiseGenerator))
                yield return GenerateRiverCoroutine();
            
            progressUI.SetTitle("Planting trees...");

            yield return null;
            if (!(terrainNoise is TestTerrainNoiseGenerator))
                yield return GenerateTrees();
            
            progressUI.SetTitle("Generating relics...");

            yield return null;
            
            if (!(terrainNoise is TestTerrainNoiseGenerator))
                yield return GeneratePyramids();
            
            var chunk = chunkManager.FindChunk(x, z);
            var chunkLocalPos = chunk.WorldToLocal(x, z);
            
            chunkManager.BakeAllChunkHeightMap();
            var y = chunk.heightMap[chunkLocalPos.x, chunkLocalPos.z] + 3;
            
            var playerPos = new Vector3(x, y, z);
            
            chunkManager.SortChunksByDistance(playerPos);
            
            progressUI.SetTitle("Generating meshes...");
            
            yield return null;
            
            progressUI.gameObject.SetActive(false);
            
            client.player.ResetPlayer(playerPos);

            var coroutine = StartCoroutine(chunkManager.ChunkManagementWorkerCoroutine());
            chunkManagementCoroutine = coroutine;

            StartCoroutine(BGMPlayCoroutine());
        }
        */

        private void GeneratePyramids()
        {
            var x = 500;
            var z = 500;

            var y = 10;
            
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

            var chunk = GetChunkFromCoord(worldX >> 4, worldY >> 4);
            chunk.BakeHeightMap();
            
            var chunkLocalPos = chunk.WorldToLocal(x, z);

            var y = chunk.heightMap[chunkLocalPos.x, chunkLocalPos.z];
            var baseBlock = GetBlock(x, y, z);

            if (check && baseBlock.blockCode != BlockMetaDatabase.BuiltinBlockCode.Grass)
                return;

            var orkCode = BlockMetaDatabase.GetBlockMetaById("b2nd:block/ork").blockCode;
            var leavesCode = BlockMetaDatabase.GetBlockMetaById("b2nd:block/leaves").blockCode;

            SetBlock(orkCode, x, ++y, z, false, false, record: false);
            SetBlock(orkCode, x, ++y, z, false, false, record: false);
            SetBlock(orkCode, x, ++y, z, false, false, record: false);

            SetBlock(leavesCode, x - 1, y, z - 1, false, false, record: false);
            SetBlock(leavesCode, x, y, z - 1, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z - 1, false, false, record: false);
            SetBlock(leavesCode, x - 1, y, z, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z, false, false, record: false);
            SetBlock(leavesCode, x - 1, y, z + 1, false, false, record: false);
            SetBlock(leavesCode, x, y, z + 1, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z + 1, false, false, record: false);

            SetBlock(leavesCode, x + 2, y, z - 1, false, false, record: false);
            SetBlock(leavesCode, x + 2, y, z, false, false, record: false);
            SetBlock(leavesCode, x + 2, y, z + 1, false, false, record: false);

            SetBlock(leavesCode, x - 2, y, z + 1, false, false, record: false);
            SetBlock(leavesCode, x - 2, y, z, false, false, record: false);
            SetBlock(leavesCode, x - 2, y, z - 1, false, false, record: false);

            SetBlock(leavesCode, x - 1, y, z + 2, false, false, record: false);
            SetBlock(leavesCode, x, y, z + 2, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z + 2, false, false, record: false);

            SetBlock(leavesCode, x - 1, y, z - 2, false, false, record: false);
            SetBlock(leavesCode, x, y, z - 2, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z - 2, false, false, record: false);

            SetBlock(orkCode, x, ++y, z, false, false, record: false);

            SetBlock(leavesCode, x, y, z + 1, false, false, record: false);
            SetBlock(leavesCode, x, y, z - 1, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z, false, false, record: false);
            SetBlock(leavesCode, x - 1, y, z, false, false, record: false);

            SetBlock(leavesCode, x + 2, y, z - 1, false, false, record: false);
            SetBlock(leavesCode, x + 2, y, z, false, false, record: false);
            SetBlock(leavesCode, x + 2, y, z + 1, false, false, record: false);

            SetBlock(leavesCode, x - 2, y, z + 1, false, false, record: false);
            SetBlock(leavesCode, x - 2, y, z, false, false, record: false);
            SetBlock(leavesCode, x - 2, y, z - 1, false, false, record: false);

            SetBlock(leavesCode, x - 1, y, z + 2, false, false, record: false);
            SetBlock(leavesCode, x, y, z + 2, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z + 2, false, false, record: false);

            SetBlock(leavesCode, x - 1, y, z - 2, false, false, record: false);
            SetBlock(leavesCode, x, y, z - 2, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z - 2, false, false, record: false);

            SetBlock(orkCode, x, ++y, z, false, false, record: false);

            SetBlock(leavesCode, x, y, z + 1, false, false, record: false);
            SetBlock(leavesCode, x, y, z - 1, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z, false, false, record: false);
            SetBlock(leavesCode, x - 1, y, z, false, false, record: false);
            SetBlock(leavesCode, x, y, z, false, false, record: false);

            SetBlock(orkCode, x, ++y, z, false, false, record: false);

            SetBlock(leavesCode, x, y, z + 1, false, false, record: false);
            SetBlock(leavesCode, x, y, z - 1, false, false, record: false);
            SetBlock(leavesCode, x + 1, y, z, false, false, record: false);
            SetBlock(leavesCode, x - 1, y, z, false, false, record: false);
            SetBlock(leavesCode, x, y, z, false, false, record: false);
        }

        private IEnumerator GenerateRiverCoroutine()
        {
            var progressUI = client.guiCanvasManager.worldGeneratingProgressUI;

            var width = worldSettings.levelWidth;
            var height = worldSettings.chunkHeight;
            
            chunkManager.BakeAllChunkHeightMap();

            var count = 1;
            var progress = 0;
            var total = chunkManager.chunkEntries.Count;

            var waterCode = BlockMetaDatabase.GetBlockCodeById("b2nd:block/water");
            
            yield return null;

            foreach (var entry in chunkManager.chunkEntries)
            {
                var cCoord = entry.basePos;
                var chunkBlocks = entry.chunk.chunkBlocks;
                var chunkWidth = chunkBlocks.GetLength(0);
                var chunkHeight = chunkBlocks.GetLength(1);

                for (int cx = 0; cx < chunkWidth; ++cx)
                {
                    for (int cz = 0; cz < chunkWidth; ++cz)
                    {
                        if (entry.chunk.heightMap[cx, cz] > terrainNoise.waterLevel)
                            continue;

                        var rawHeight = terrainNoise.GetHeight((cCoord.x + cx) / 384f, (cCoord.z + cz) / 384f);

                        for (int cy = chunkHeight - 1; cy >= 0; --cy)
                        {
                            if (cy > terrainNoise.waterLevel)
                            {
                                chunkBlocks[cx, cy, cz].blockCode = 0;
                            } else if (cy <= terrainNoise.waterLevel && cy >= entry.chunk.heightMap[cx, cz] && 
                                       terrainNoise.waterLevel - rawHeight < 0.005f)
                            {
                                chunkBlocks[cx, cy, cz].blockCode = BlockMetaDatabase.BuiltinBlockCode.Sand;
                            } else if (cy <= terrainNoise.waterLevel && cy >= entry.chunk.heightMap[cx, cz])
                            {
                                chunkBlocks[cx, cy, cz].blockCode = waterCode;
                                chunkBlocks[cx, cy, cz].blockState = LiquidBlockBehavior.DefaultIterCount;
                            } else if (cy >= entry.chunk.heightMap[cx, cz] - 2)
                            {
                                chunkBlocks[cx, cy, cz].blockCode = BlockMetaDatabase.BuiltinBlockCode.Dirt;
                            } else
                            {
                                chunkBlocks[cx, cy, cz].blockCode = BlockMetaDatabase.BuiltinBlockCode.Stone;
                            }
                        }
                    }
                }
                
                if (count % 20 == 0)
                {
                    count = 1;
                    progressUI.SetProgress((float) progress / total);
                    yield return null;
                }
                else
                {
                    count++;
                }

                progress++;
            }
        }

        public void Explode(int x, int y, int z, int radius)
        {
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
                                SetBlock(0, cx, cy, cz, false);
                            }
                        }
                    }
                }
            }

            var dir = client.player.transform.position - new Vector3(x, y, z);
            client.player.playerController.AddImpulseForse(dir.normalized * 300 / (1f + dir.magnitude));
            
            FlushDirtyChunkQueue();
        }

        public int GetHeight(int x, int z)
        {
            var chunk = GetChunkFromCoord(x >> 4, z >> 4);
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

        public ChunkBlockData GetBlock(Vector3 pos)
        {
            return GetBlock((int) pos.x, (int) pos.y, (int) pos.z, out Chunk _);
        }
        
        public ChunkBlockData GetBlock(int x, int y, int z)
        {
            return GetBlock(x, y, z, out Chunk _);
        }

        public ChunkBlockData GetBlock(int x, int y, int z, out Chunk locatedChunk)
        {
            int chunkX = x >> 4;
            int chunkZ = z >> 4;

            var chunk = chunkProvider.ProvideChunk(this, chunkX, chunkZ);
            var local = chunk.WorldToLocal(x, y, z);

            locatedChunk = chunk;
            
            if (y < 0 || y >= worldSettings.chunkHeight || local.x < 0 || local.x >= 16 || local.z < 0 || local.z >= 16)
                return ChunkBlockData.EMPTY;

            return chunk.chunkBlocks[local.x, local.y, local.z];
        }

        public Chunk GetChunkFromCoord(int chunkX, int chunkZ)
        {
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
        
        public void SetBlock(int blockCode, int x, int y, int z, 
                                        bool updateMesh, bool updateHeightmap = true,
                                        bool notify = false, bool useInitState = true, byte state = 0,
                                        bool record = true)
        {
            int chunkX = x >> 4;
            int chunkZ = z >> 4;

            var chunk = chunkProvider.ProvideChunk(this, chunkX, chunkZ);
            var local = chunk.WorldToLocal(x, y, z);

            var meta = BlockMetaDatabase.GetBlockMetaByCode(blockCode);

            if (y >= worldSettings.chunkHeight || y < 0)
                return;

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

            if (updateMesh)
            {
                chunk.BakeHeightMap();
                chunkRenderEntityManager.RenderChunk(chunk);
            }
            else
            {
                if (!dirtyChunkPreFrameQueue.Contains(chunk) && record)
                    dirtyChunkPreFrameQueue.Enqueue(chunk);
            }

            if (updateHeightmap)
                chunk.BakeHeightMap();

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
                meta.shape.GetShapeMesh(1, 1).texcoords[0]);
            
            Destroy(particleGameObject, 2f);
        }

        public void SaveLevel()
        {
            var levelDataWriter = levelSaveHandler.GetLevelDataWriter();
            var playerDataWriter = levelSaveHandler.GetPlayerDataWriter();
            
            var levelKnbt = new KNBTTagCompound("Level");

            var player = client.player;
            var playerKnbt = player.GetPlayerKNBTData();

            levelKnbt.SetInt("Version", 1);
            levelKnbt.SetString("Name", name);
            levelKnbt.SetInt("Type", chunkProvider.GetChunkGenerator().GetId());
            
            levelKnbt.Write(levelDataWriter);
            playerKnbt.Write(playerDataWriter);
            
            playerDataWriter.Dispose();
            levelDataWriter.Dispose();
            
            chunkProvider.SaveChunk(this, true);
        }
    }
}