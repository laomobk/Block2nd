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
    }
}