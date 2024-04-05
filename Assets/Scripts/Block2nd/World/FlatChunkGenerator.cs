using Block2nd.Database;
using Block2nd.Scriptable;
using UnityEngine;

namespace Block2nd.World
{
    public class FlatChunkGenerator : ChunkGeneratorBase
    {
        private WorldSettings worldSettings;
        private TerrainNoiseGenerator noiseGenerator;

        public FlatChunkGenerator(WorldSettings worldSettings)
        {
            this.worldSettings = worldSettings;
            noiseGenerator = new FlatTerrainNoiseGenerator(worldSettings);
        }
        
        public override Chunk GenerateChunk(Level level, int chunkX, int chunkZ)
        {
            int height = worldSettings.chunkHeight;
            
            ChunkBlockData[,,] blocks = new ChunkBlockData[16, worldSettings.chunkHeight, 16];
            GenerateBasicTerrain(worldSettings, noiseGenerator, blocks, chunkX, chunkZ, false);

            Chunk chunk = new Chunk(level, chunkX, chunkZ, worldSettings.chunkHeight);
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
            
        }

        public override int GetId()
        {
            return 1;
        }

        public override int GetBaseHeight()
        {
            return noiseGenerator.baseHeight;
        }
    }
}