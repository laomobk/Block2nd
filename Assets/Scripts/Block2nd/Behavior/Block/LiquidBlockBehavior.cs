using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Behavior.Block
{
    public abstract class LiquidBlockBehavior : BlockBehavior
    {
        private static readonly byte DefaultIterCount = 8;
        
        protected abstract int GetSelfBlockCode();

        public override void OnPlace(ref IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            var local = chunk.WorldToLocal(worldPos.x, worldPos.y, worldPos.z);
            chunk.SetBlockState(local.x, local.y, local.z, DefaultIterCount, false, false);
            OnUpdate(worldPos, level, chunk, player);
        }

        public override void OnUpdate(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            byte state = chunk.GetBlock(originalPos).blockState;
            byte iterCount = (byte) (state & 16);
            
            Debug.Log(iterCount);
            
            if (iterCount == 0)
            {
                return;
            }

            var x = originalPos.x;
            var y = originalPos.y;
            var z = originalPos.z;

            Chunk cp;

            byte newState = (byte) (state - 1);
            
            if (y > 0 && level.GetBlock(x, y - 1, z, out cp).blockCode == 0)
            {
                level.SetBlock(
                    GetSelfBlockCode(), x, y - 1, z, false, state: newState);

                level.ChunkManager.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x, y - 1, z),
                    onlyUpdateCenterBlock = true
                });
                
                return;
            }
            
            if (level.GetBlock(x, y, z + 1, out cp).blockCode == 0)
            {
                level.SetBlock(
                    GetSelfBlockCode(), x, y, z + 1, false, state: newState);

                level.ChunkManager.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x, y, z + 1),
                    onlyUpdateCenterBlock = true
                });
            }
            
            if (level.GetBlock(x, y, z - 1, out cp).blockCode == 0)
            {
                level.SetBlock(
                    GetSelfBlockCode(), x, y, z - 1, false, state: newState);

                level.ChunkManager.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x, y, z - 1),
                    onlyUpdateCenterBlock = true
                });
            }
            
            if (level.GetBlock(x + 1, y, z, out cp).blockCode == 0)
            {
                level.SetBlock(
                    GetSelfBlockCode(), x + 1, y, z, false, state: newState);

                level.ChunkManager.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x + 1, y, z),
                    onlyUpdateCenterBlock = true
                });
            }
            
            if (level.GetBlock(x - 1, y, z, out cp).blockCode == 0)
            {
                level.SetBlock(
                    GetSelfBlockCode(), x - 1, y, z, false, state: newState);

                level.ChunkManager.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x - 1, y, z),
                    onlyUpdateCenterBlock = true
                });
            }
        }
    }
}