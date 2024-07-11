using Block2nd.MathUtil;
using Unity.Collections;
using Unity.Jobs;

namespace Block2nd.World
{
    public struct PreheatSurroundingChunkJob : IJobParallelFor
    {
        public NativeArray<IntVector3> positions;
        
        public void Execute(int index)
        {
            
        }
    }
}