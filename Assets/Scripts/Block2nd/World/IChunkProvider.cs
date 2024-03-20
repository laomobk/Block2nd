namespace Block2nd.World
{
    public interface IChunkProvider
    {
        Chunk ProvideChunk(Level level, int chunkX, int chunkZ);
    }
}