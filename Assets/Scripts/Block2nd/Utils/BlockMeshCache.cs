using System.Collections.Generic;
using Block2nd.Database.Meta;

namespace Block2nd.Utils
{
    public class BlockMeshCache
    {
        private static readonly Dictionary<string, BlockMesh> CacheDict = new Dictionary<string, BlockMesh>();
        
        public static BlockMesh GetByKey(string key)
        {
            return CacheDict.TryGetValue(key, out var mesh) ? mesh : null;
        }

        public static void Add(string key, BlockMesh blockMesh)
        {
            CacheDict.Add(key, blockMesh);
        }
    }
}