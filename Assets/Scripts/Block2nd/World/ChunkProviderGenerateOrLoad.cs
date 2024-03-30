using System.Collections.Generic;
using Block2nd.MathUtil;
using UnityEngine;

namespace Block2nd.World
{
    public class ChunkProviderGenerateOrLoad : IChunkProvider
    {
        private readonly IChunkLoader chunkLoader;
        private readonly ChunkGeneratorBase chunkGenerator;

        private Dictionary<ulong, Chunk> chunkDict = new Dictionary<ulong, Chunk>();

        private Chunk[] hotChunks;

        public ChunkProviderGenerateOrLoad(IChunkLoader chunkLoader, ChunkGeneratorBase chunkGenerator)
        {
            this.chunkLoader = chunkLoader;
            this.chunkGenerator = chunkGenerator;
            
            hotChunks = new Chunk[1024];
        }

        public void UnloadChunk(Chunk chunk)
        {
            chunk.chunkBlocks = null;
            
            chunkDict.Remove(chunk.CoordKey);
        }

        private void ReleaseHotChunk(int chunkX, int chunkZ, ulong key, Level level, Chunk chunk)
        {
            int hotIdx = (chunkX & 0x1f) * 32 + (chunkZ & 0x1f);

            if (hotChunks[hotIdx] != null && hotChunks[hotIdx].CoordKey != key)
            {
                var oldChunk = hotChunks[hotIdx];
                chunkLoader.SaveChunk(level, oldChunk);
                UnloadChunk(oldChunk);
            }
            
            hotChunks[hotIdx] = chunk;
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
                ReleaseHotChunk(chunkX, chunkZ, key, level, chunk);
                return chunk;
            }

            chunk = chunkGenerator.GenerateChunk(level, chunkX, chunkZ);
                
            chunkDict.Add(key, chunk);

            ReleaseHotChunk(chunkX, chunkZ, key, level, chunk);

            if (chunk.populateState < 1)
            {
                chunkGenerator.PopulateChunk(level, chunkX, chunkZ);
                chunk.populateState = 1;
            }

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
            
            // save 1 chunks.
            foreach (var chunk in hotChunks)
            {
                if (chunk != null && chunk.NeedToSave)
                {
                    chunkLoader.SaveChunk(level, chunk);
                    break;
                }
            }
        }

        public ChunkGeneratorBase GetChunkGenerator()
        {
            return chunkGenerator;
        }

        public IChunkLoader GetChunkLoader()
        {
            return chunkLoader;
        }
    }
}