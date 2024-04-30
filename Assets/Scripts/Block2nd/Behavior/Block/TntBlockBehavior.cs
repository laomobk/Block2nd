using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.World;
using UnityEngine;
using Random = System.Random;

namespace Block2nd.Behavior.Block
{
    public class TntBlockBehavior : BlockBehavior
    {
        Random random = new Random();
        
        public override BlockBehavior CreateInstance()
        {
            return this;
        }

        public override bool OnInteract(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            var soundIdx = random.Next(1, 4);
            level.PlaySoundAt("sound3/random/explode" + soundIdx, originalPos.x, originalPos.y, originalPos.z);
            level.Explode(originalPos.x, originalPos.y, originalPos.z, 5);

            return false;
        }

        public override bool OnHurt(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            return OnInteract(originalPos, level, chunk, player);
        }
    }
}