using System;
using Block2nd.Phys;

namespace Block2nd.Entity
{
    public class EntityWithBehavior : EntityBase
    {
        public EntityBehavior behavior;

        private void Update()
        {
            behavior?.Update(this);
        }

        public override AABB GetAABB()
        {
            return behavior.GetAABB();
        }
    }
}