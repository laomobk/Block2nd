using Block2nd.Phys;
using UnityEngine;

namespace Block2nd.Behavior.Block
{
    public class NullBlockBehavior : BlockBehavior
    {
        public static readonly NullBlockBehavior Default = new NullBlockBehavior();
        
        private static NullBlockBehavior instance = null;
        public override BlockBehavior CreateInstance()
        {
            if (instance == null)
                instance = new NullBlockBehavior();
            return instance;
        }

        public override bool CanRaycast()
        {
            return false;
        }
    }
}