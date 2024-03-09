using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.World;

namespace Block2nd.Behavior.Block
{
    public class SandBlockBehavior : BlockBehavior
    {
        public override BlockBehavior CreateInstance()
        {
            return this;
        } 

        public override void OnPlace(ref IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            
            int height = level.GetHeight(originalPos.x, originalPos.z);

            originalPos.y = height + 1;
        }
    }
}