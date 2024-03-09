using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Behavior.Block
{
    public abstract class LiquidBlockBehavior : BlockBehavior
    {
        public static readonly int defaultIterCount = 8;
        public int iterCount;
        public IntVector3 source;
        public bool inLoss = false;

        public LiquidBlockBehavior()
        {
            iterCount = defaultIterCount;
        }

        protected abstract int GetSelfBlockCode();

        public override void OnPlace(ref IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            OnUpdate(originalPos, level, chunk, player);
        }

        public override void OnUpdate(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            if (iterCount <= 0)
            {
                return;
            }

            var x = originalPos.x;
            var y = originalPos.y;
            var z = originalPos.z;

            var source = iterCount != defaultIterCount ? this.source : originalPos;

            Chunk cp;
            
            if (y > 0 && level.GetBlock(x, y - 1, z, out cp).blockCode == 0)
            {
                var behavior = level.SetBlock(
                    GetSelfBlockCode(), x, y - 1, z, false);

                if (behavior is LiquidBlockBehavior blockBehavior)
                {
                    blockBehavior.iterCount = iterCount;
                    blockBehavior.source = source;
                    level.ChunkManager.AddUpdateToNextTick(new ChunkUpdateContext
                    {
                        chunk = cp,
                        pos = new IntVector3(x, y - 1, z),
                        onlyUpdateCenterBlock = true
                    });
                }
                
                return;
            }
            
            if (level.GetBlock(x, y, z + 1, out cp).blockCode == 0)
            {
                var behavior = level.SetBlock(
                    GetSelfBlockCode(), x, y, z + 1, false);

                if (behavior is LiquidBlockBehavior blockBehavior)
                {
                    blockBehavior.iterCount = iterCount - 1;
                    blockBehavior.source = source;
                    level.ChunkManager.AddUpdateToNextTick(new ChunkUpdateContext
                    {
                        chunk = cp,
                        pos = new IntVector3(x, y, z + 1),
                        onlyUpdateCenterBlock = true
                    });
                }
            }
            
            if (level.GetBlock(x, y, z - 1, out cp).blockCode == 0)
            {
                var behavior = level.SetBlock(
                    GetSelfBlockCode(), x, y, z - 1, false);

                if (behavior is LiquidBlockBehavior blockBehavior)
                {
                    blockBehavior.iterCount = iterCount - 1;
                    blockBehavior.source = source;
                    level.ChunkManager.AddUpdateToNextTick(new ChunkUpdateContext
                    {
                        chunk = cp,
                        pos = new IntVector3(x, y, z - 1),
                        onlyUpdateCenterBlock = true
                    });
                }
            }
            
            if (level.GetBlock(x + 1, y, z, out cp).blockCode == 0)
            {
                var behavior = level.SetBlock(
                    GetSelfBlockCode(), x + 1, y, z, false);

                if (behavior is LiquidBlockBehavior blockBehavior)
                {
                    blockBehavior.iterCount = iterCount - 1;
                    blockBehavior.source = source;
                    level.ChunkManager.AddUpdateToNextTick(new ChunkUpdateContext
                    {
                        chunk = cp,
                        pos = new IntVector3(x + 1, y, z),
                        onlyUpdateCenterBlock = true
                    });
                }
            }
            
            if (level.GetBlock(x - 1, y, z, out cp).blockCode == 0)
            {
                var behavior = level.SetBlock(
                    GetSelfBlockCode(), x - 1, y, z, false);

                if (behavior is LiquidBlockBehavior blockBehavior)
                {
                    blockBehavior.iterCount = iterCount - 1;
                    blockBehavior.source = source;
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
}