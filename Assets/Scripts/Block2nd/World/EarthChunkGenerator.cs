using Block2nd.Database;
using Block2nd.Scriptable;
using UnityEngine;
using UnityEngine.Profiling;
using Random = System.Random;

namespace Block2nd.World
{
    public class EarthChunkGenerator : ChunkGeneratorBase
    {
        private WorldSettings worldSettings;
        private TerrainNoiseGenerator noiseGenerator;

        private IBiome biome;

        public EarthChunkGenerator(WorldSettings worldSettings)
        {
            this.worldSettings = worldSettings;
            this.noiseGenerator = new TerrainNoiseGenerator(worldSettings);

            biome = new BiomePlain();
        }

        private void GenerateCurve(ChunkBlockData[,,] blocks, int chunkX, int chunkZ)
        {
            var worldX = chunkX << 4;
            var worldZ = chunkZ << 4;
            
            for (int x = 0; x < 16; ++x)
            {
                for (int z = 0; z < 16; ++z)
                {
                    if (baseHeight[x, z] + 2 < noiseGenerator.waterLevel)
                    {
                        continue;
                    }
                    
                    for (int y = 95; y >= 30; --y)
                    {
                        float value = noiseGenerator.GetCheese3D((worldX + x) / 128f, y / 128f, (worldZ + z) / 128f);

                        value *= (y - 30) / 65f;

                        if (value > 0.02f)
                        {
                            blocks[x, y, z].blockCode = 0;
                            blocks[x, y, z].blockState = 0;
                        }
                    }
                }
            }
        }
        
        public override Chunk GenerateChunk(Level level, int chunkX, int chunkZ)
        {
            int height = worldSettings.chunkHeight;
            
            ChunkBlockData[,,] blocks = new ChunkBlockData[16, worldSettings.chunkHeight, 16];
            GenerateBasicTerrain(worldSettings, noiseGenerator, blocks, chunkX, chunkZ);

            Chunk chunk = new Chunk(level, chunkX, chunkZ);
            chunk.chunkBlocks = blocks;
            chunk.aabb = new Bounds(
                new Vector3(8, height / 2f, 8),
                new Vector3(16, height, 16)
            );
            chunk.BakeHeightMap();
            
            // GenerateCurve(blocks, chunkX, chunkZ);

            return chunk;
        }

        public override void PopulateChunk(Level level, int chunkX, int chunkZ)
        {
            biome.Decorate(level, chunkX << 4, chunkZ << 4);
        }

        public override int GetId()
        {
            return 0;
        }

        public override int GetBaseHeight()
        {
            return noiseGenerator.baseHeight;
        }
    }
}