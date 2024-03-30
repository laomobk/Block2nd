using System.Runtime.CompilerServices;
using UnityEngine;

namespace Block2nd.MathUtil
{
    public static class MathHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistancePlane(Vector3 v1, Vector3 v2)
        {
            v1.y = 0;
            v2.y = 0;

            return (v1 - v2).magnitude;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorToLevelIndex(float x, float[] levels)
        {
            if (x < levels[0])
                return 0;
            
            for (int i = 0; i < levels.Length; ++i)
            {
                if (levels[i] >= x)
                {
                    return i;
                }
            }

            return levels.Length - 1;
        }
        
        public static int FloorToLevelIndex(float x, int[] levels)
        {
            if (x < levels[0])
                return 0;
            
            for (int i = 0; i < levels.Length; ++i)
            {
                if (levels[i] >= x)
                {
                    return i;
                }
            }

            return levels.Length - 1;
        }
    }
}