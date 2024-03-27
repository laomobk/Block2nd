using Block2nd.Database;

namespace Block2nd.World
{
    public static class BuiltinChunkGeneratorFactory
    {
        public static ChunkGeneratorBase GetChunkGeneratorFromId(int id, WorldSettings worldSettings)
        {
            switch (id)
            {
                case 1:
                    return new FlatChunkGenerator(worldSettings);
                case 2:
                    return new HonkaiChunkGenerator(worldSettings);
            }
            
            return new EarthChunkGenerator(worldSettings);
        }
    }
}