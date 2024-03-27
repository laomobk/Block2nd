using System.Collections.Generic;
using Block2nd.MathUtil;
using UnityEngine;

namespace Block2nd.World
{
    public class ChunkProviderGenerateOrLoad : IChunkProvider
    {
        private readonly IChunkLoader chunkLoader;
        private readonly ChunkGeneratorBase chunkGeneratorBase;

        private Dictionary<ulong, Chunk> chunkDict = new Dictionary<ulong, Chunk>();

        private Chunk[] hotChunks;

        public ChunkProviderGenerateOrLoad(IChunkLoader chunkLoader, ChunkGeneratorBase chunkGeneratorBase)
        {
            this.chunkLoader = chunkLoader;
            this.chunkGeneratorBase = chunkGeneratorBase;

            hotChunks = new Chunk[1024];
        }
        
        public Chunk ProvideChunk(Level level, int chunkX, int chunkZ)
        {
            var key = ChunkHelper.ChunkCoordsToLongKey(chunkX, chunkZ);
            Chunk chunk;

            if (chunkDict.TryGetValue(key, out chunk))
            {
                return chunk;
            }

            chunk = chunkLoader.TryLoadChunk(level, chunkX, chunkZ);

            if (chunk != null)
            {
                chunkDict.Add(key, chunk);
                return chunk;
            }

            chunk = chunkGeneratorBase.GenerateChunk(level, chunkX, chunkZ);
                
            chunkDict.Add(key, chunk);
            
            chunkGeneratorBase.PopulateChunk(level, chunkX, chunkZ);
            
            chunk.BakeHeightMap();

            return chunk;
        }

        public Chunk TryGetChunk(Level level, int chunkX, int chunkZ)
        {
            var key = ChunkHelper.ChunkCoordsToLongKey(chunkX, chunkZ);
            Chunk chunk;

            if (chunkDict.TryGetValue(key, out chunk))
            {
                return chunk;
            }
            
            chunk = chunkLoader.TryLoadChunk(level, chunkX, chunkZ);

            if (chunk != null)
                return chunk;

            return null;
        }

        public int GetChunkCacheCount()
        {
            return chunkDict.Count;
        }

        public void SaveChunk(Level level, bool isSaveAll)
        {
            if (isSaveAll)
            {
                int c = 0;
                
                foreach (var chunk in chunkDict.Values)
                {
                    if (chunk.NeedToSave)
                    {
                        chunkLoader.SaveChunk(level, chunk);
                        c++;
                    } 
                }
                
                Debug.Log("Chunk Provider: " + c + " chunk(s) saved");

                return;
            }
        }
    }
}