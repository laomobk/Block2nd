using Block2nd.Database.Meta;
using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.World;

namespace Block2nd.Behavior
{
    public abstract class BlockBehavior
    {
        public abstract BlockBehavior CreateInstance();

        public virtual bool OnDestroy(Level level, Chunk chunk, Player player)
        {
            return true;
        }

        public virtual void OnTick(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            
        }

        public virtual void OnPlace(ref IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
        }

        public virtual bool OnInteract(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            return true;
        }
        
        public virtual void OnUpdate(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            
        }

        public virtual bool OnHurt(IntVector3 originalPos, Level level, Chunk chunk, Player player)
        {
            return true;
        }

        public virtual BlockMesh OnRender()
        {
            return null;
        }
    }

    public class StaticBlockBehavior : BlockBehavior
    {
        public static StaticBlockBehavior Default = new StaticBlockBehavior();
        
        public override BlockBehavior CreateInstance()
        {
            return this;
        }
    }
}