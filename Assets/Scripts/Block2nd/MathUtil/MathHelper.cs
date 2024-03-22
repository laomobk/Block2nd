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
    }
}