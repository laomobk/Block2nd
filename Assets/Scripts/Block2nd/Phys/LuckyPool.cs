using System.Collections.Generic;

namespace Block2nd.Phys
{
    public static class LuckyPool
    {
        private static readonly int MaxPoolCount = 300;
        private static List<AABB> aabbPool = new List<AABB>();
        private static int pointer = 0;

        static LuckyPool()
        {
            for (int i = 0; i < MaxPoolCount; ++i)
            {
                aabbPool.Add(new AABB(0, 0, 0, 0, 0, 0));
            }
        }

        public static AABB GetAABBFromPool(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            pointer = (pointer + 1) % MaxPoolCount;
            return aabbPool[pointer].Set(minX, minY, minZ, maxX, maxY, maxZ);
        }

        public static void ClearPool()
        {
            pointer = 0;
        }
    }
}