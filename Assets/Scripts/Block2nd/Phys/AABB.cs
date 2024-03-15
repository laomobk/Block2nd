using System;
using UnityEngine;

namespace Block2nd.Phys
{
    public class AABB
    {
        public float epsilon = 0;
        public float minX, minY, minZ;
        public float maxX, maxY, maxZ;

        public Vector3 Center => new Vector3(
            minX + (maxX - minX) / 2, 
            minY + (maxY - minY) / 2, 
            minZ + (maxZ - minZ) / 2);

        public AABB(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;
            this.maxX = maxX;
            this.maxY = maxY;
            this.maxZ = maxZ;
        }
        
        public AABB Set(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;
            this.maxX = maxX;
            this.maxY = maxY;
            this.maxZ = maxZ;

            return this;
        }

        public void Move(float mx, float my, float mz)
        {
            minX += mx;
            minY += my;
            minZ += mz;
            maxX += mx;
            maxY += my;
            maxZ += mz;
        }

        public AABB Copy()
        {
            return new AABB(minX, minY, minZ, maxX, maxY, maxZ);
        }

        public AABB CopyWithOffset(float ox, float oy, float oz)
        {
            return new AABB(minX + ox, minY + oy, minZ + oz, maxX + ox, maxY + oy, maxZ + oz);
        }

        public AABB CopyWithExpand(float dx, float dy, float dz)
        {
            var newMinX = minX;
            var newMinY = minY;
            var newMinZ = minZ;
            var newMaxX = maxX;
            var newMaxY = maxY;
            var newMaxZ = maxZ;

            if (dx > 0)
                newMaxX += dx;
            else
                newMinX += dx;

            if (dy > 0)
                newMaxY += dy;
            else
                newMinY += dy;

            if (dz > 0)
                newMaxZ += dz;
            else
                newMinZ += dz;

            return new AABB(newMinX, newMinY, newMinZ, newMaxX, newMaxY, newMaxZ);
        }

        public float ClipXCollide(AABB thatBox, float wantX)
        {
            if (thatBox.maxY > minY && thatBox.minY < maxY && thatBox.maxZ > minZ && thatBox.minZ < maxZ)
            {
                float actualX;
                if (wantX > 0 && thatBox.maxX <= minX && (actualX = minX - thatBox.maxX - epsilon) < wantX)
                {
                    if (actualX < 0)  // avoid bouncing
                        actualX = 0;
                    wantX = actualX;
                }

                if (wantX < 0 && thatBox.minX >= maxX && (actualX = thatBox.minX - maxX + epsilon) > wantX)
                {
                    if (actualX > 0)  // avoid bouncing
                        actualX = 0;
                    wantX = actualX;
                }
            }

            return wantX;
        }

        public float ClipYCollide(AABB thatBox, float wantY)
        {
            
            if (thatBox.maxX > minX && thatBox.minX < maxX && thatBox.maxZ > minZ && thatBox.minZ < maxZ)
            {
                float actualY;
                if (wantY > 0 && thatBox.maxY <= minY && (actualY = minY - thatBox.maxY - epsilon) < wantY)
                {
                    if (actualY < 0)  // avoid bouncing
                        actualY = 0;
                    wantY = actualY;
                } else if (wantY < 0 && thatBox.minY >= maxY && (actualY = thatBox.minY - maxY + epsilon) > wantY)
                {
                    if (actualY > 0)  // avoid bouncing
                        actualY = 0;
                    wantY = actualY;
                }
            }
            return wantY;
        }

        public float ClipZCollide(AABB thatBox, float wantZ)
        {
            if (thatBox.maxY > minY && thatBox.minY < maxY && thatBox.maxX > minX && thatBox.minX < maxX)
            {
                float actualZ;
                if (wantZ > 0 && thatBox.maxZ <= minZ && (actualZ = minZ - thatBox.maxZ - epsilon) < wantZ)
                {
                    if (actualZ < 0)  // avoid bouncing
                        actualZ = 0;
                    wantZ = actualZ;
                }

                if (wantZ < 0 && thatBox.minZ >= maxZ && (actualZ = thatBox.minZ - maxZ + epsilon) > wantZ)
                {
                    if (actualZ > 0)  // avoid bouncing
                        actualZ = 0;
                    wantZ = actualZ;
                }
            }

            return wantZ;
        }

        public bool Intersects(AABB thatBox)
        {
            return thatBox.maxX > this.minX && thatBox.minX <= this.maxX && 
                   thatBox.maxY > this.minY && thatBox.minY <= this.maxY && 
                   thatBox.maxZ > this.minZ && thatBox.minZ <= this.maxZ;
        }

        public bool Intersects(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            return maxX > this.minX && minX <= this.maxX && 
                   maxY > this.minY && minY <= this.maxY && 
                   maxZ > this.minZ && minZ <= this.maxZ;
        }

        public bool Contains(AABB thatBox)
        {
            return this.minX > thatBox.minX && this.maxX > thatBox.maxX &&
                   this.minY > thatBox.maxY && this.maxY > thatBox.maxY &&
                   this.minZ > thatBox.minZ && this.maxZ > thatBox.maxZ;
        }

        public bool Contains(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            return this.minX > minX && this.maxX > maxX &&
                   this.minY > maxY && this.maxY > maxY &&
                   this.minZ > minZ && this.maxZ > maxZ;
        }

        public bool InSideXYBounds(Vector3 point)
        {
            return point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
        }
        
        public bool InSideXZBounds(Vector3 point)
        {
            return point.x >= minX && point.x <= maxX && point.z >= minZ && point.z <= maxZ;
        }

        public bool InSideYZBounds(Vector3 point)
        {
            return point.z >= minZ && point.z <= maxZ && point.y >= minY && point.y <= maxY;
        }

        /// <summary>
        ///     检测向量 start -> end 是否穿过或穿入了此碰撞箱
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public RayHit Raycast(Vector3 start, Vector3 end)
        {

            bool hitYZ1 = ScaleToXPlane(start, end, minX, out Vector3 YZ1);
            bool hitYZ2 = ScaleToXPlane(start, end, maxX, out Vector3 YZ2);
            bool hitXY1 = ScaleToZPlane(start, end, minZ, out Vector3 XY1);
            bool hitXY2 = ScaleToZPlane(start, end, maxZ, out Vector3 XY2);
            bool hitXZ1 = ScaleToYPlane(start, end, minY, out Vector3 XZ1);
            bool hitXZ2 = ScaleToYPlane(start, end, maxY, out Vector3 XZ2);

            bool gotHitPoint = false;
            Vector3 hitPoint = start + (end - start) * 9999999999f;

            if (hitXY1 && InSideXYBounds(XY1) && (hitPoint - start).sqrMagnitude > (XY1 - start).sqrMagnitude)
            {
                gotHitPoint = true;
                hitPoint = XY1;
            }
            
            if (hitXY2 && InSideXYBounds(XY2) && (hitPoint - start).sqrMagnitude > (XY2 - start).sqrMagnitude)
            {
                gotHitPoint = true;
                hitPoint = XY2;
            }
            
            if (hitXZ1 && InSideXZBounds(XZ1) && (hitPoint - start).sqrMagnitude > (XZ1 - start).sqrMagnitude)
            {
                gotHitPoint = true;
                hitPoint = XZ1;
            }
            
            if (hitXZ2 && InSideXZBounds(XZ2) && (hitPoint - start).sqrMagnitude > (XZ2 - start).sqrMagnitude)
            {
                gotHitPoint = true;
                hitPoint = XZ2;
            }
            
            if (hitYZ1 && InSideYZBounds(YZ1) && (hitPoint - start).sqrMagnitude > (YZ1 - start).sqrMagnitude)
            {
                gotHitPoint = true;
                hitPoint = YZ1;
            }
            
            if (hitYZ2 && InSideYZBounds(YZ2) && (hitPoint - start).sqrMagnitude > (YZ2 - start).sqrMagnitude)
            {
                gotHitPoint = true;
                hitPoint = YZ2;
            }

            if (!gotHitPoint)
                return null;

            byte direction = 0;

            if (hitPoint == XY1)
                direction = 2;
            else if (hitPoint == XY2)
                direction = 1;
            else if (hitPoint == XZ1)
                direction = 6;
            else if (hitPoint == XZ2)
                direction = 5;
            else if (hitPoint == YZ1)
                direction = 3;
            else if (hitPoint == YZ2)
                direction = 4;

            return new RayHit((int) minX, (int) minY, (int) minZ, direction, hitPoint);
        }

        /// <summary>
        ///     返回沿 start -> end 方向的，缩放至 plane 位置的向量
        /// </summary>
        /// <returns> false 如果不存在这样的向量 </returns>
        public bool ScaleToZPlane(Vector3 start, Vector3 end, float plane, out Vector3 result)
        {
            var dx = end.x - start.x;
            var dy = end.y - start.y;
            var dz = end.z - start.z;
            
            result = Vector3.zero;

            if (dz * dz < Single.Epsilon)
                return false;

            var scale = (plane - start.z) / dz;
            if (scale < 0 || scale > 1)  // make sure the plane is between the start and end.
                return false;

            result = new Vector3(start.x + dx * scale, start.y + dy * scale, start.z + dz * scale);
            return true;
        }
        
        /// <summary>
        ///     返回沿 start -> end 方向的，缩放至 plane 位置的向量
        /// </summary>
        /// <returns> false 如果不存在这样的向量 </returns>
        public bool ScaleToYPlane(Vector3 start, Vector3 end, float plane, out Vector3 result)
        {
            var dx = end.x - start.x;
            var dy = end.y - start.y;
            var dz = end.z - start.z;
            
            result = Vector3.zero;

            if (dy * dy < Single.Epsilon)
                return false;

            var scale = (plane - start.y) / dy;
            if (scale < 0 || scale > 1)  // make sure the plane is between the start and end.
                return false;

            result = new Vector3(start.x + dx * scale, start.y + dy * scale, start.z + dz * scale);
            return true;
        }
        
        /// <summary>
        ///     返回沿 start -> end 方向的，缩放至 plane 位置的向量
        /// </summary>
        /// <returns> false 如果不存在这样的向量 </returns>
        public bool ScaleToXPlane(Vector3 start, Vector3 end, float plane, out Vector3 result)
        {
            var dx = end.x - start.x;
            var dy = end.y - start.y;
            var dz = end.z - start.z;
            
            result = Vector3.zero;

            if (dx * dx < Single.Epsilon)
                return false;

            var scale = (plane - start.x) / dx;
            if (scale < 0 || scale > 1)  // make sure the plane is between the start and end.
                return false;

            result = new Vector3(start.x + dx * scale, start.y + dy * scale, start.z + dz * scale);
            return true;
        }

        public string ToString()
        {
            return "(" + minX + ", " + minY + ", " + minZ + ", " + maxX + ", " + maxY + ", " + maxZ + ")";
        }

        public static AABB One()
        {
            return new AABB(0, 0, 0, 1, 1, 1);
        }

        public static readonly AABB LazyOneBox = new AABB(0, 0, 0, 1, 1, 1);
    }
}