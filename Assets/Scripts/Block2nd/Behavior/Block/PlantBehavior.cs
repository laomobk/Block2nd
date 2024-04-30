using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.World;

namespace Block2nd.Behavior.Block
{
    public class PlantBehavior: BlockBehavior
    {
        public override BlockBehavior CreateInstance()
        {
            return this;
        }

        public override bool CanCollide()
        {
            return false;
        }

        public override void OnUpdate(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            var below = worldPos;
            below.y -= 1;
            if (level.GetBlock(below.x, below.y, below.z, false, true).blockCode == 0)
            {
                level.DestroyBlock(worldPos.x, worldPos.y, worldPos.z);
            }
        }
    }
}