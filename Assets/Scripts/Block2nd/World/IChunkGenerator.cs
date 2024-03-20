namespace Block2nd.World
{
    public interface IChunkGenerator
    {
        Chunk GenerateChunk(Level level, int chunkX, int chunkY);
    }
}