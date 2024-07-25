namespace Block2nd.World
{
    public interface IChunkLoader
    {
        Chunk TryLoadChunk(Level level, int chunkX, int chunkZ);

        void SaveChunk(Level level, Chunk chunk);

        void Clean();
    }
}