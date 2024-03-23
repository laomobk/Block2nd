using System;
using Block2nd.Phys;
using UnityEngine;

namespace Block2nd.Entity
{
    public class PlayerEntity : Entity
    {
        public float gravity = -0.98f;
        public float verticalVelocity = 0;
        
        private AABB standardAABB = new AABB(0.1f, 0.1f, 0.1f, 0.9f, 1.7f, 0.9f);
        private AABB aabb = new AABB(0.1f, 0.1f, 0.1f, 0.9f, 1.7f, 0.9f);
        public override AABB GetAABB()
        {
            return aabb;
        }
        
        public override AABB GetStandardAABB()
        {
            return standardAABB;
        }

        public void Jump()
        {
            verticalVelocity += 5f;
        }

        private void Update()
        {
            
        }
    }
}