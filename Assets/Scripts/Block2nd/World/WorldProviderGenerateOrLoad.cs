using System.Collections.Generic;
using Block2nd.MathUtil;

namespace Block2nd.World
{
    public class WorldProviderGenerateOrLoad : IChunkProvider
    {
        private readonly IChunkLoader chunkLoader;
        private readonly IChunkGenerator chunkGenerator;

        private Dictionary<long, Chunk> chunkDict;

        public WorldProviderGenerateOrLoad(IChunkLoader chunkLoader, IChunkGenerator chunkGenerator)
        {
            this.chunkLoader = chunkLoader;
            this.chunkGenerator = chunkGenerator;
        }

        public long ChunkCoordsToLongKey(int chunkX, int chunkZ)
        {
            long key = (long) (chunkX >> 4) << 32 | (uint) (chunkZ >> 4);

            return key;
        }

        public Chunk ProvideChunk(Level level, int chunkX, int chunkZ)
        {
            var key = ChunkCoordsToLongKey(chunkX, chunkZ);
            Chunk chunk;
            
            if (chunkDict.TryGetValue(key, out chunk))
            {
                return chunk;
            }
            
            chunk = chunkLoader.TryLoadChunk(chunkX, chunkZ);

            if (chunk != null)
                return chunk;

            chunk = chunkGenerator.GenerateChunk(level, chunkX, chunkZ);
            chunkDict.Add(key, chunk);

            return chunk;
        }
    }
}