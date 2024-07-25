using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Block2nd.World
{
    internal class ChunkRegion
    {
        public Chunk[] chunksTable = new Chunk[1024];
        public int savedCounter = 0;

        public static ChunkRegion FromStream(Stream stream)
        {
            var reader = new BinaryReader(stream);

            return null;
        }
    }
    
    public class LocalChunkLoaderRegion : IChunkLoader
    {
        private Dictionary<uint, ChunkRegion> regionCacheTable = new Dictionary<uint, ChunkRegion>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint CalcRegionCoordsKey(int chunkX, int chunkZ)
        {
            return ((((uint) chunkX >> 20) & 0xffff) << 16) | (((uint) chunkZ >> 20) & 0xffff);
        }
        
        private ChunkRegion LoadRegion(int chunkX, int chunkZ)
        {
            var key = CalcRegionCoordsKey(chunkX, chunkZ);

            if (regionCacheTable.TryGetValue(key, out var region))
            {
                return region;
            }
            
            // TODO: Load Region From Disk.

            region = new ChunkRegion();
            regionCacheTable.Add(key, region);

            return region;
        }
        
        public Chunk TryLoadChunk(Level level, int chunkX, int chunkZ)
        {
            return null;
        }

        public void SaveChunk(Level level, Chunk chunk)
        {
            
        }

        public void Clean()
        {
            throw new System.NotImplementedException();
        }
    }
}