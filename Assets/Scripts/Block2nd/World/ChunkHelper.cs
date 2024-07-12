using UnityEngine;

namespace Block2nd.World
{
    public class ChunkHelper
    {
        public static ulong ChunkCoordsToLongKey(int chunkX, int chunkZ)
        {
            uint uX = (uint) chunkX;
            uint uZ = (uint) chunkZ;
            
            ulong key = ((ulong)uX << 32) | uZ;

            return key;
        }
    }
}