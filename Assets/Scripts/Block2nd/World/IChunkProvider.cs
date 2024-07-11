namespace Block2nd.World
{
    public interface IChunkProvider
    {
        Chunk PreheatChunk(Level level, int chunkX, int chunkZ);
        
        Chunk ProvideChunk(Level level, int chunkX, int chunkZ);

        Chunk TryGetChunk(Level level, int chunkX, int chunkZ);

        Chunk GetChunkInCache(Level level, int chunkX, int chunkZ);

        int GetChunkCacheCount();

        void SaveChunk(Level level, bool isSaveAll);

        ChunkGeneratorBase GetChunkGenerator();

        IChunkLoader GetChunkLoader();
    }
}