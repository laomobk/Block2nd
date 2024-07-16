using Block2nd.Phys;
using UnityEngine;

namespace Block2nd.Behavior.Block
{
    public class NullBlockBehavior : BlockBehavior
    {
        public static readonly NullBlockBehavior Default = new NullBlockBehavior();
        
        private static NullBlockBehavior _instance = null;
        public override BlockBehavior CreateInstance()
        {
            if (_instance == null)
                _instance = new NullBlockBehavior();
            return _instance;
        }

        public override bool CanRaycast()
        {
            return false;
        }
    }
}