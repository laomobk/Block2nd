using System;
using Block2nd.Client;
using Block2nd.Phys;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Entity
{
    public abstract class Entity : MonoBehaviour
    {
        protected float stepHeight = 1;
        protected bool onGround = false;

        protected GameClient gameClient;

        public abstract AABB GetAABB();

        public virtual AABB GetStandardAABB()
        {
            return GetAABB();
        }
        
        public bool OnGround => onGround;

        private void Awake()
        {
            gameClient = GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>();
        }
        
        public void MoveAABBToWorldPosition()
        {
            var standardAABB = GetStandardAABB();
            var aabb = GetAABB();
            
            var halfX = (standardAABB.maxX - standardAABB.minX) / 2f;
            var halfY = (standardAABB.maxY - standardAABB.minY) / 2f;
            var halfZ = (standardAABB.maxZ - standardAABB.minZ) / 2f;
            var worldPos = transform.position;

            aabb.minX = worldPos.x - halfX;
            aabb.minY = worldPos.y - halfY;
            aabb.minZ = worldPos.z - halfZ;
            aabb.maxX = worldPos.x + halfX;
            aabb.maxY = worldPos.y + halfY;
            aabb.maxZ = worldPos.z + halfZ;
        }

        public void MoveWorldPositionToAABB()
        {
            transform.position = GetAABB().Center;
        }
        
        public void MoveWorld(Vector3 dir)
        {
            float wantX = dir.x;
            float wantY = dir.y;
            float wantZ = dir.z;

            float dx = wantX;
            float dy = wantY;
            float dz = wantZ;

            var aabb = GetAABB();
            
            var collideBoxes = gameClient.CurrentLevel.GetWorldCollideBoxIntersect(
                                                aabb.CopyWithExpand(wantX, wantY, wantZ));

            for (int i = 0; i < collideBoxes.Count; ++i)
            {
                var box = collideBoxes[i];
                dy = box.ClipYCollide(aabb, dy);
            }
            
            aabb.Move(0, dy, 0);

            onGround = dy != wantY && wantY < 0;

            foreach (var box in collideBoxes)
            {
                dx = box.ClipXCollide(aabb, dx);
                dz = box.ClipZCollide(aabb, dz);
            }
            
            aabb.Move(dx, 0f, dz);
            
            MoveWorldPositionToAABB();
        }
    }
}