namespace Block2nd.World
{
    public interface IChunkProvider
    {
        Chunk ProvideChunk(Level level, int chunkX, int chunkZ);

        Chunk TryGetChunk(int chunkX, int chunkZ);

        int GetChunkCacheCount();
    }
}