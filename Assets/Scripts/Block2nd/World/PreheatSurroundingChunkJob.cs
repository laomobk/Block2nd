using Block2nd.MathUtil;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Block2nd.World
{
    public struct PreheatSurroundingChunkJob : IJobParallelFor
    {
        public int offset;
        public NativeArray<IntVector3> positions;
        public NativeArray<int> newChunkFlagArray;

        public void Execute(int index)
        {
            var pos = positions[offset + index];
            var chunk = ChunkJobExchange.chunkProvider.PreheatChunk(ChunkJobExchange.level, pos.x, pos.y);
            if (chunk.populateState < 1)
            {
                newChunkFlagArray[offset + index] = 1;
            }
        }
    }
}