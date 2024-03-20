using Block2nd.Database;
using UnityEngine;
using Random = System.Random;

namespace Block2nd.World
{
    public class EarthChunkGenerator : IChunkGenerator
    {
        private Random random = new Random();
        
        private WorldSettings worldSettings;
        private TerrainNoiseGenerator noiseGenerator;

        public EarthChunkGenerator(WorldSettings worldSettings, TerrainNoiseGenerator noiseGenerator)
        {
            this.worldSettings = worldSettings;
            this.noiseGenerator = noiseGenerator;
        }
        
        private int GetHeight(float x, float z)
        {
            return (int)noiseGenerator.GetHeight(x / 384, z / 384);
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

        private void GenerateBasicTerrain(ChunkBlockData[,,] blocks, int chunkX, int chunkZ)
        {
            var height = worldSettings.chunkHeight;
            var worldX = chunkX << 4;
            var worldZ = chunkZ << 4;
            
            for (int x = 0; x < 16; x += 16)
            {
                for (int z = 0; z < 16; z += 16)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        var blockCode = GetBlockCodeFromGenerator(worldX + x, y, worldZ + z);
                        byte initState = BlockMetaDatabase.GetBlockMetaByCode(blockCode).initState;
                        blocks[x, y, z] = new ChunkBlockData
                        {
                            blockCode = blockCode,
                            blockState = initState
                        };
                    }
                }
            }
        }
        
        public Chunk GenerateChunk(Level level, int chunkX, int chunkZ)
        {
            int height = worldSettings.chunkHeight;
            
            ChunkBlockData[,,] blocks = new ChunkBlockData[16, worldSettings.chunkHeight, 16];
            GenerateBasicTerrain(blocks, chunkX, chunkZ);

            Chunk chunk = new Chunk(level.ChunkManager, chunkX, chunkZ);
            chunk.chunkBlocks = blocks;
            chunk.aabb = new Bounds(
                new Vector3(8, height / 2f, 8),
                new Vector3(16, height, 16)
            );
            chunk.BakeHeightMap();

            return chunk;
        }
    }
}