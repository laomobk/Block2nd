using System;
using System.Xml.Schema;
using Block2nd.Client;
using Block2nd.Phys;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Entity
{
    public abstract class EntityBase : MonoBehaviour
    {
        protected float stepHeight = 1;
        protected bool onGround = false;
        protected bool hitFront = false;
        protected bool hitTop = false;

        protected GameClient gameClient;

        public abstract AABB GetAABB();

        public virtual AABB GetStandardAABB()
        {
            return GetAABB();
        }
        
        [HideInInspector] public Vector3 forward = Vector3.zero;
        
        public bool OnGround => onGround;
        public bool HitFront => hitFront;
        public bool HitTop => hitTop;

        private void Awake()
        {
            var go = GameObject.FindGameObjectWithTag("GameClient");
            if (go != null)
                gameClient = go.GetComponent<GameClient>();
            else
                Debug.LogWarning("Game Client instance not found!");
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
            hitTop = dy != wantY && wantY > 0;

            foreach (var box in collideBoxes)
            {
                dx = box.ClipXCollide(aabb, dx);
                dz = box.ClipZCollide(aabb, dz);
            }
            
            var dVec = new Vector3(dx, dy, dz);

            hitFront = Vector3.Dot(forward.normalized, dir.normalized) > 0.95 && 
                       (dx != wantX || dz != wantZ) && dVec.magnitude < 0.65 * Time.deltaTime;
            
            aabb.Move(dx, 0f, dz);
            
            MoveWorldPositionToAABB();
        }
    }
}