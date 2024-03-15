using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Behavior;
using Block2nd.Client;
using Block2nd.Render;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.Phys;
using UnityEngine;
using UnityEngine.Profiling;

namespace Block2nd.World
{
    public class Level : MonoBehaviour
    {
        public string levelName = "Level_01";
        
        public System.Random random = new System.Random();

        private ChunkManager chunkManager;
        private TerrainGenerator terrain;

        public WorldSettings worldSettings;

        public GameObject blockParticlePrefab;
        public GameObject chunkPrefab;

        public TerrainGenerator TerrainGenerator => terrain;

        public Func<int, int, float> terrainHeightFunc;

        private GameClient client;
        private Coroutine chunkManagementCoroutine;

        private float tickInterval = 0.3f;
        private float lastTickTime = -10f;

        public Vector3 gravity = new Vector3(0, -0.98f, 0);

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
            chunkManager = new ChunkManager(chunkPrefab, transform, worldSettings, client);
            terrain = new TerrainGenerator(worldSettings);
        }
        
        private void OnDestroy()
        {
            chunkManager.chunkWorkerRunning = false;
            if (chunkManagementCoroutine != null)
                StopCoroutine(chunkManagementCoroutine);
        }

        private void LateUpdate()
        {
            if (Time.time - lastTickTime > tickInterval)
            {
                var nUpdate = chunkManager.PerformChunkUpdate();
                lastTickTime = Time.time;
                client.guiCanvasManager.SetUpdateText(nUpdate);
            }
        }

        public IEnumerator CreateLevelCoroutine(TerrainGenerator generator = null)
        {
            if (generator != null)
            {
                this.terrain = generator;
            }
            
            Resources.UnloadUnusedAssets();
            
            var progressUI = client.guiCanvasManager.worldGeneratingProgressUI;
            
            var levelWidth = worldSettings.levelWidth;
            
            int x = levelWidth / 2;
            int z = levelWidth / 2;
            
            progressUI.gameObject.SetActive(true);
            
            progressUI.SetTitle("Generating terrain...");

            yield return null;
            
            yield return GenerateLevelBlocksCoroutine();
            
            progressUI.SetTitle("Making rivers...");
            
            if (!(terrain is FlatTerrainGenerator) && !(terrain is HonkaiTerrainGenerator))
                yield return GenerateRiverCoroutine();
            
            progressUI.SetTitle("Planting trees...");

            yield return null;
            if (!(terrain is TestTerrainGenerator))
                yield return GenerateTrees();
            
            progressUI.SetTitle("Generating relics...");

            yield return null;
            
            if (!(terrain is TestTerrainGenerator))
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

            /*
            
            yield return chunkManager.RenderAllChunkMesh(() =>
            {
            });
            
            */

            /*
            yield return null;

            yield return null;
            
            yield return new WaitForSeconds(0.5f);
            */
            // chunkManager.BakeAllChunkHeightMap();
        }

        private IEnumerator GeneratePyramids()
        {
            var progressUI = client.guiCanvasManager.worldGeneratingProgressUI;
            
            var x = random.Next(40, worldSettings.levelWidth - 40);
            var z = random.Next(40, worldSettings.levelWidth - 40);

            var chunk = chunkManager.FindChunk(x, z);
            var chunkLocalPos = chunk.WorldToLocal(x, z);
                
            var y = terrain is FlatTerrainGenerator ? 5 : 10;
            
            var redBrickCode = BlockMetaDatabase.GetBlockMetaById("b2nd:block/red_brick").blockCode;

            for (int h = 0; h < 32; ++h)
            {
                for (int px = -32 + h; px < 32 - h; ++px)
                {
                    for (int pz = -32 + h; pz < 32 - h; ++pz)
                    {
                        SetBlock(redBrickCode, x + px, y + h, z + pz, false);
                    }
                }
             
                progressUI.SetProgress((h + 1) / 32f);
                yield return null;   
            }
        }

        private IEnumerator GenerateTrees()
        {
            var progressUI = client.guiCanvasManager.worldGeneratingProgressUI;
            var waterCode = BlockMetaDatabase.GetBlockCodeById("b2nd:block/water");

            var nTree = terrain is FlatTerrainGenerator ? 200 : 425;
            
            for (int i = 0; i < nTree; ++i)
            {
                var x = random.Next(5, worldSettings.levelWidth - 5);
                var z = random.Next(5, worldSettings.levelWidth - 5);

                var chunk = chunkManager.FindChunk(x, z);
                var chunkLocalPos = chunk.WorldToLocal(x, z);
                
                var y = chunk.heightMap[chunkLocalPos.x, chunkLocalPos.z];
                
                if (GetBlock(x, y, z).blockCode == waterCode)
                    continue;

                var orkCode = BlockMetaDatabase.GetBlockMetaById("b2nd:block/ork").blockCode;
                var leavesCode = BlockMetaDatabase.GetBlockMetaById("b2nd:block/leaves").blockCode;

                SetBlock(orkCode, x, ++y, z, false, false);
                SetBlock(orkCode, x, ++y, z, false, false);
                SetBlock(orkCode, x, ++y, z, false, false);

                SetBlock(leavesCode, x - 1, y, z - 1, false, false);
                SetBlock(leavesCode, x, y, z - 1, false, false);
                SetBlock(leavesCode, x + 1, y, z - 1, false, false);
                SetBlock(leavesCode, x - 1, y, z, false, false);
                SetBlock(leavesCode, x + 1, y, z, false, false);
                SetBlock(leavesCode, x - 1, y, z + 1, false, false);
                SetBlock(leavesCode, x, y, z + 1, false, false);
                SetBlock(leavesCode, x + 1, y, z + 1, false, false);
                
                SetBlock(leavesCode, x + 2, y, z - 1, false, false);
                SetBlock(leavesCode, x + 2, y, z, false, false);
                SetBlock(leavesCode, x + 2, y, z + 1, false, false);
                
                SetBlock(leavesCode, x - 2, y, z + 1, false, false);
                SetBlock(leavesCode, x - 2, y, z, false, false);
                SetBlock(leavesCode, x - 2, y, z - 1, false, false);
                
                SetBlock(leavesCode, x - 1, y, z + 2, false, false);
                SetBlock(leavesCode, x, y, z + 2, false, false);
                SetBlock(leavesCode, x + 1, y, z + 2, false, false);
                
                SetBlock(leavesCode, x - 1, y, z - 2, false, false);
                SetBlock(leavesCode, x, y, z - 2, false, false);
                SetBlock(leavesCode, x + 1, y, z - 2, false, false);
                
                SetBlock(orkCode, x, ++y, z, false, false);
                
                SetBlock(leavesCode, x, y, z + 1, false, false);
                SetBlock(leavesCode, x, y, z - 1, false, false);
                SetBlock(leavesCode, x + 1, y, z, false, false);
                SetBlock(leavesCode, x - 1, y, z, false, false);
                
                SetBlock(leavesCode, x + 2, y, z - 1, false, false);
                SetBlock(leavesCode, x + 2, y, z, false, false);
                SetBlock(leavesCode, x + 2, y, z + 1, false, false);
                
                SetBlock(leavesCode, x - 2, y, z + 1, false, false);
                SetBlock(leavesCode, x - 2, y, z, false, false);
                SetBlock(leavesCode, x - 2, y, z - 1, false, false);
                
                SetBlock(leavesCode, x - 1, y, z + 2, false, false);
                SetBlock(leavesCode, x, y, z + 2, false, false);
                SetBlock(leavesCode, x + 1, y, z + 2, false, false);
                
                SetBlock(leavesCode, x - 1, y, z - 2, false, false);
                SetBlock(leavesCode, x, y, z - 2, false, false);
                SetBlock(leavesCode, x + 1, y, z - 2, false, false);
                
                SetBlock(orkCode, x, ++y, z, false, false);
                
                SetBlock(leavesCode, x, y, z + 1, false, false);
                SetBlock(leavesCode, x, y, z - 1, false, false);
                SetBlock(leavesCode, x + 1, y, z, false, false);
                SetBlock(leavesCode, x - 1, y, z, false, false);
                SetBlock(leavesCode, x, y, z, false, false);
                
                SetBlock(orkCode, x, ++y, z, false, false);
                
                SetBlock(leavesCode, x, y, z + 1, false, false);
                SetBlock(leavesCode, x, y, z - 1, false, false);
                SetBlock(leavesCode, x + 1, y, z, false, false);
                SetBlock(leavesCode, x - 1, y, z, false, false);
                SetBlock(leavesCode, x, y, z, false, false);

                if (i % 55 == 0)
                {
                    progressUI.SetProgress((i + 1f) / nTree);
                    yield return null;
                }
            }
        }

        private IEnumerator GenerateLevelBlocksCoroutine()
        {
            var progressUI = client.guiCanvasManager.worldGeneratingProgressUI;
            
            var width = worldSettings.levelWidth;
            var height = worldSettings.chunkHeight;

            yield return null;

            for (int x = 0; x < width; x += worldSettings.chunkWidth)
            {
                for (int z = 0; z < width; z += worldSettings.chunkWidth)
                {
                    var chunk = chunkManager.AddNewChunkGameObject(x, z);
                    var chunkBlocks = chunk.chunkBlocks;
                    var chunkWidth = chunkBlocks.GetLength(0);
                    var chunkHeight = chunkBlocks.GetLength(1);

                    for (int cx = 0; cx < chunkWidth; ++cx)
                    {
                        for (int cz = 0; cz < chunkWidth; ++cz)
                        {
                            for (int cy = 0; cy < chunkHeight; ++cy)
                            {
                                var blockCode = GetBlockCodeFromGenerator(x + cx, cy, z + cz);
                                chunkBlocks[cx, cy, cz] = new ChunkBlockData
                                {
                                    blockCode = blockCode
                                };
                            }
                        }
                    }
                }
                progressUI.SetProgress((float) x / width);
                yield return null;
            }
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
                        if (entry.chunk.heightMap[cx, cz] > terrain.waterLevel)
                            continue;

                        var rawHeight = terrain.GetHeight((cCoord.x + cx) / 384f, (cCoord.z + cz) / 384f);

                        for (int cy = chunkHeight - 1; cy >= 0; --cy)
                        {
                            if (cy > terrain.waterLevel)
                            {
                                chunkBlocks[cx, cy, cz].blockCode = 0;
                            } else if (cy <= terrain.waterLevel && cy >= entry.chunk.heightMap[cx, cz] && 
                                       terrain.waterLevel - rawHeight < 0.005f)
                            {
                                chunkBlocks[cx, cy, cz].blockCode = BlockMetaDatabase.BuiltinBlockCode.Sand;
                            } else if (cy <= terrain.waterLevel && cy >= entry.chunk.heightMap[cx, cz])
                            {
                                chunkBlocks[cx, cy, cz].blockCode = waterCode;
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
            
            chunkManager.SortChunksByDistance(client.player.transform.position);
            chunkManager.ForceBeginChunksManagement();
        }

        public int GetHeight(int x, int z)
        {
            var chunk = chunkManager.FindChunk(x, z);
            var chunkLocalPos = chunk.WorldToLocal(x, z);
                
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

        private int GetHeight(float x, float z)
        {
            return (int)terrain.GetHeight(x / 384, z / 384);
        }

        int GetBlockCodeFromGenerator(float x, float y, float z)
        {
            float height = GetHeight(x, z);
            float curY = y;

            if (curY > height)
            {
                return 0;
            }
            else if (curY == height)
            {
                return BlockMetaDatabase.BuiltinBlockCode.Grass;
            }
            else if (curY >= height - random.Next(0, 2))
            {
                return BlockMetaDatabase.BuiltinBlockCode.Dirt;
            }
            else
            {
                return BlockMetaDatabase.BuiltinBlockCode.Stone;
            }
        }

        public static bool Check(GameObject obj)
        {
            return obj.name == "Chuck";
        }

        public ChunkBlockData GetBlock(Vector3 pos)
        {
            return GetBlock((int) pos.x, (int) pos.y, (int) pos.z, out Chunk _);
        }
        
        public ChunkBlockData GetBlock(Vector3 pos, out Chunk locatedChunk)
        {
            return GetBlock((int) pos.x, (int) pos.y, (int) pos.z, out locatedChunk);
        }

        public ChunkBlockData GetBlock(int x, int y, int z)
        {
            return GetBlock(x, y, z, out Chunk _);
        }

        public ChunkBlockData GetBlock(int x, int y, int z, out Chunk locatedChunk)
        {
            return chunkManager.GetBlock(x, y, z, out locatedChunk);
        }

        public void SetBlockState(int x, int y, int z, byte state, bool updateMesh)
        {
            chunkManager.SetBlockState(x, y, z, state, updateMesh);
        }

        public void SetBlock(int blockCode, Vector3 pos, bool update,
                                        bool updateHeightmap = true, bool triggerUpdate = true,
                                        byte state = 0)
        {
            var intPos = new IntVector3(pos);
            
            chunkManager.SetBlock(blockCode, intPos.x, intPos.y, intPos.z, 
                update, updateHeightmap, triggerUpdate, state);
        }
        
        public void SetBlock(int blockCode, int x, int y, int z, 
                                        bool updateMesh, bool updateHeightmap = true,
                                        bool triggerUpdate = false, byte state = 0)
        {
            chunkManager.SetBlock(blockCode, x, y, z, updateMesh, updateHeightmap, triggerUpdate, state);
        }

        public RayHit RaycastBlocks(Vector3 start, Vector3 end)
        {
            start += new Vector3(5e-5f, 5e-5f, -5e-5f);
            int iStartX = MathHelper.FloorInt(start.x);
            int iStartY = MathHelper.FloorInt(start.y);
            int iStartZ = MathHelper.FloorInt(start.z);
            int iEndX = MathHelper.FloorInt(end.x);
            int iEndY = MathHelper.FloorInt(end.y);
            int iEndZ = MathHelper.FloorInt(end.z);

            RayHit hit;
            ChunkBlockData block = GetBlock(iStartX, iStartY, iStartZ);
            var blockBehavior = BlockMetaDatabase.GetBlockBehaviorByCode(block.blockCode);
            
            if (blockBehavior.CanRaycast() &&
                (hit = blockBehavior.GetAABB(iStartX, iStartY, iStartZ).Raycast(start, end)) != null)
            {
                return hit;
            }

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

                byte normalDirection;
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
                iStartX = MathHelper.FloorInt(start.x);
                iStartY = MathHelper.FloorInt(start.y);
                iStartZ = MathHelper.FloorInt(start.z);

                if (normalDirection == RayHitNormalDirection.Right)
                    iStartX--;
                
                if (normalDirection == RayHitNormalDirection.Up) // if top face, offset it.
                    iStartY--;
                
                if (normalDirection == RayHitNormalDirection.Forward)  // if front face, offset it.
                    iStartZ--;

                blockBehavior = BlockMetaDatabase.GetBlockBehaviorByCode(
                    GetBlock(iStartX, iStartY, iStartZ).blockCode);
                if (blockBehavior.CanRaycast() && 
                    (hit = blockBehavior.GetAABB(iStartX, iStartY, iStartZ).Raycast(start, end)) != null)
                {
                    return hit;
                }
            }
            
            return null;
        }

        public List<AABB> GetWorldCollideBoxIntersect(AABB aabb)
        {
            var x0 = (int) aabb.minX;
            var x1 = (int) aabb.maxX + 1;
            var y0 = (int) aabb.minY;
            var y1 = (int) aabb.maxY + 1;
            var z0 = (int) aabb.minZ;
            var z1 = (int) aabb.maxZ + 1;

            if (x0 < 0)
                x0--;

            if (y0 < 0)
                y0--;

            if (z0 < 0)
                z0--;

            var result = new List<AABB>();

            for (int x = x0; x < x1; ++x)
            {
                for (int y = y0; y < y1; ++y)
                {
                    for (int z = z0; z < z1; ++z)
                    {
                        var block = GetBlock(x, y, z);
                        AABB blockBox;
                        if (block.blockCode != 0 &&  
                            !BlockMetaDatabase.GetBlockMetaByCode(block.blockCode).liquid &&
                            aabb.Intersects(blockBox = BlockMetaDatabase.GetBlockBehaviorByCode(block.blockCode)
                                .GetAABB(new IntVector3(x, y, z))))
                        {
                            result.Add(blockBox);
                        }
                    }
                }
            }

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
    }
}