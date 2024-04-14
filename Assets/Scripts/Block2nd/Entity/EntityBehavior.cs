using Block2nd.Phys;
using UnityEngine;

namespace Block2nd.Entity
{
    public abstract class EntityBehavior
    {
        protected AABB aabb = AABB.LazyOneBox;

        protected void SetSize(int width, int height)
        {
            aabb = new AABB(0, 0, 0, width, height, width);
        }
        
        public virtual void Update(EntityBase self) {}

        public AABB GetAABB()
        {
            return aabb;
        }
    }
}