using System.Runtime.CompilerServices;

namespace Block2nd.MathUtil
{
    public static class MathHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorInt(double x)
        {
            return x > 0 ? (int) x : (int) x - 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorInt(float x)
        {
            return x > 0 ? (int) x : (int) x - 1;
        }
    }
}