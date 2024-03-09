using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Behavior.Block
{
    public class TntBlockBehavior : BlockBehavior
    {
        public override BlockBehavior CreateInstance()
        {
            return this;
        }

        public override bool OnInteract(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            level.Explode(originalPos.x, originalPos.y, originalPos.z, 5);

            return false;
        }

        public override bool OnHurt(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            return OnInteract(originalPos, level, chunk, player);
        }
    }
}