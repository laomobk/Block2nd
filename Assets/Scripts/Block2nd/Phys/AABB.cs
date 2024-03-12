namespace Block2nd.Phys
{
    public class AABB
    {
        public float epsilon = 0.001f;
        public float minX, minY, minZ;
        public float maxX, maxY, maxZ;
        
        public AABB(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;
            this.maxX = maxX;
            this.maxY = maxY;
            this.maxZ = maxZ;
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

        public float ClipXCollide(AABB thatBox, float wantX)
        {
            if (thatBox.maxY > minY && thatBox.minY < maxY && thatBox.maxZ > minZ && thatBox.minZ < maxZ)
            {
                float actualX;
                if (wantX > 0 && thatBox.maxX <= minX && (actualX = minX - thatBox.maxX - epsilon) < wantX)
                    wantX = actualX;
                if (wantX < 0 && thatBox.minX >= maxX && (actualX = thatBox.minX - maxX + epsilon) > wantX)
                    wantX = actualX;
            }

            return wantX;
        }

        public float ClipYCollide(AABB thatBox, float wantY)
        {
            if (thatBox.maxX > minX && thatBox.minX < maxX && thatBox.maxZ > minZ && thatBox.minZ < maxZ)
            {
                float actualY;
                if (wantY > 0 && thatBox.maxY <= minY && (actualY = minY - thatBox.maxY - epsilon) < wantY)
                    wantY = actualY;
                if (wantY < 0 && thatBox.minY >= maxY && (actualY = thatBox.minY - maxY + epsilon) > wantY)
                    wantY = actualY;
            }

            return wantY;
        }

        public float ClipZCollide(AABB thatBox, float wantZ)
        {
            if (thatBox.maxY > minY && thatBox.minY < maxY && thatBox.maxX > minX && thatBox.minX < maxX)
            {
                float actualZ;
                if (wantZ > 0 && thatBox.maxZ <= minZ && (actualZ = minZ - thatBox.maxZ - epsilon) < wantZ)
                    wantZ = actualZ;
                if (wantZ < 0 && thatBox.minZ >= maxZ && (actualZ = thatBox.minZ - maxZ + epsilon) > wantZ)
                    wantZ = actualZ;
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

        public static AABB One()
        {
            return new AABB(0, 0, 0, 1, 1, 1);
        }
    }
}