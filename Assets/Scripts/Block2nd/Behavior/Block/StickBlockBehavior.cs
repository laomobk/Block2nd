using Block2nd.Audio;
using Block2nd.Phys;

namespace Block2nd.Behavior.Block
{
    public class StickBlockBehavior : BlockBehavior
    {
        public StickBlockBehavior(BlockSoundDescriptor descriptor) : base(descriptor) {}
        
        public override BlockBehavior CreateInstance()
        {
            return this;
        }

        public override bool CanCollide()
        {
            return false;
        }

        public override AABB GetAABB(int x, int y, int z)
        {
            return LuckyPool.GetAABBFromPool(
                x + 0.4f, y, z + 0.4f,
                x + 0.525f, y + 0.625f, z + 0.525f);
        }
    }
}