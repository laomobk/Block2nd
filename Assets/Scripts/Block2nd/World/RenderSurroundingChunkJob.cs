using Block2nd.MathUtil;
using Unity.Collections;
using Unity.Jobs;

namespace Block2nd.World
{
    struct RenderSurroundingChunkJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<IntVector3> positions;

        public void Execute(int index)
        {
            var pos = positions[index];
            ChunkJobExchange.chunkRenderEntityManager.TryRenderChunkInJob(
                ChunkJobExchange.chunkProvider.TryGetChunk(
                    ChunkJobExchange.level, pos.x, pos.y),
                ChunkJobExchange.playerPos);
        }
    }
}