using Block2nd.Database;
using Block2nd.Scriptable;
using UnityEngine;

namespace Block2nd.World
{
    public abstract class ChunkGeneratorBase
    {
        protected System.Random random = new System.Random();

        protected virtual int GetHeight(TerrainNoiseGenerator noiseGenerator, float x, float z)
        {
            return (int)noiseGenerator.GetHeight((x - 12550821) / 384, (z - 12550821) / 384);
        }
        
        protected virtual int GetBlockCodeFromGenerator(
                                TerrainNoiseGenerator noiseGenerator, float y, float noiseHeight,
                                bool water = true)
        {
            var waterLevel = water ? noiseGenerator.waterLevel : -10000;

            float curY = y;
            
            if (curY > Mathf.Max(noiseHeight, waterLevel))
            {
                return 0;
            }

            if (water)
            {
                if (curY <= waterLevel && curY > noiseHeight)
                {
                    return BlockMetaDatabase.BuiltinBlockCode.Water;
                }

                if (curY >= waterLevel && noiseHeight >= waterLevel && noiseHeight - waterLevel < 1f)
                {
                    return BlockMetaDatabase.BuiltinBlockCode.Sand;
                }
            }

            if (curY == noiseHeight)
            {
                if (curY < waterLevel && water)
                    return BlockMetaDatabase.BuiltinBlockCode.Dirt;
                return BlockMetaDatabase.BuiltinBlockCode.Grass;
            }
            
            if (curY >= noiseHeight - random.Next(0, 2))
            {
                return BlockMetaDatabase.BuiltinBlockCode.Dirt;
            }
            
            return BlockMetaDatabase.BuiltinBlockCode.Stone;
        }

        protected virtual void GenerateBasicTerrain(
            WorldSettings worldSettings, TerrainNoiseGenerator noiseGenerator,
            ChunkBlockData[,,] blocks, int chunkX, int chunkZ, bool water = true)
        {
            var height = worldSettings.chunkHeight;
            var worldX = chunkX << 4;
            var worldZ = chunkZ << 4;
            float noiseHeight;
            
            for (int x = 0; x < 16; ++x)
            {
                for (int z = 0; z < 16; ++z)
                {
                    noiseHeight = GetHeight(noiseGenerator, worldX + x, worldZ + z);
                    
                    for (int y = 0; y < height; ++y)
                    {
                        var blockCode = GetBlockCodeFromGenerator(
                            noiseGenerator,y, noiseHeight, water);
                        byte initState = blockCode > 0 ? 
                                            BlockMetaDatabase.GetBlockMetaByCode(blockCode).initState : 
                                            (byte)0;
                        blocks[x, y, z] = new ChunkBlockData
                        {
                            blockCode = blockCode,
                            blockState = initState
                        };
                    }
                }
            }
        }
        
        public abstract Chunk GenerateChunk(Level level, int chunkX, int chunkZ);

        public abstract void PopulateChunk(Level level, int chunkX, int chunkZ);

        public abstract int GetId();

        public abstract int GetBaseHeight();
    }
}