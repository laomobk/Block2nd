using System;
using Block2nd.Client;
using Block2nd.Phys;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Entity
{
    public class Entity : MonoBehaviour
    {
        protected float stepHeight = 1;
        protected AABB aabb = AABB.One();
        protected bool onGround = false;

        [HideInInspector] public Vector3 velocity;
        [HideInInspector] public Vector3 gravityVelocity;

        protected GameClient gameClient;

        private void Awake()
        {
            gameClient = GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>();
        }

        public void ResetVelocity()
        {
            velocity = Vector3.zero;
            gravityVelocity = Vector3.zero;
        }

        protected void ApplyGravity(ref Vector3 dir)
        {
            if (!onGround)
            {
                gravityVelocity += gameClient.CurrentLevel.gravity * Time.deltaTime;
                dir += gravityVelocity;
            }
            else
            {
                gravityVelocity = Vector3.zero;
            }
        }

        public void MoveAABBToWorldPosition()
        {
            var halfX = (aabb.maxX - aabb.minX) / 2f;
            var halfY = (aabb.maxY - aabb.minY) / 2f;
            var halfZ = (aabb.maxZ - aabb.minZ) / 2f;
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
            transform.position = aabb.Center;
        }

        public void MoveRelative(Vector3 dir)
        {   
            ApplyGravity(ref dir);
            
            var wantX = dir.x;
            var wantY = dir.y;
            var wantZ = dir.z;

            var dx = wantX;
            var dy = wantY;
            var dz = wantZ;
            
            var collideBoxes = gameClient.CurrentLevel.GetWorldCollideBoxIntersect(
                                                aabb.CopyWithExpand(wantX, wantY, wantZ));

            foreach (var box in collideBoxes)
            {
                dy = box.ClipYCollide(aabb, dy);
                dx = box.ClipXCollide(aabb, dx);
                dz = box.ClipZCollide(aabb, dz);
            }

            if (Math.Abs(wantY - dy) > Single.Epsilon)
                onGround = true;
            else
                onGround = false;
            
            aabb.Move(dx, dy, dz);
            
            MoveWorldPositionToAABB();
        }
    }
}