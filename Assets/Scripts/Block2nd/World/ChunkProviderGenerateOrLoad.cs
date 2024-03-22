using System.Collections.Generic;
using Block2nd.MathUtil;
using UnityEngine;

namespace Block2nd.World
{
    public class ChunkProviderGenerateOrLoad : IChunkProvider
    {
        private readonly IChunkLoader chunkLoader;
        private readonly IChunkGenerator chunkGenerator;

        private Dictionary<ulong, Chunk> chunkDict = new Dictionary<ulong, Chunk>();

        public ChunkProviderGenerateOrLoad(IChunkLoader chunkLoader, IChunkGenerator chunkGenerator)
        {
            this.chunkLoader = chunkLoader;
            this.chunkGenerator = chunkGenerator;
        }
        
        public Chunk ProvideChunk(Level level, int chunkX, int chunkZ)
        {
            var key = ChunkHelper.ChunkCoordsToLongKey(chunkX, chunkZ);
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

        public Chunk TryGetChunk(int chunkX, int chunkZ)
        {
            var key = ChunkHelper.ChunkCoordsToLongKey(chunkX, chunkZ);
            Chunk chunk;

            if (chunkDict.TryGetValue(key, out chunk))
            {
                return chunk;
            }
            
            chunk = chunkLoader.TryLoadChunk(chunkX, chunkZ);

            if (chunk != null)
                return chunk;

            return null;
        }

        public int GetChunkCacheCount()
        {
            return chunkDict.Count;
        }
    }
}