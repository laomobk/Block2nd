namespace Block2nd.World
{
    public interface IChunkLoader
    {
        Chunk TryLoadChunk(int chunkX, int chunkZ);
    }
}