using System;
using UnityEngine;

namespace Block2nd.MathUtil
{
    [Serializable]
    public struct IntVector3
    {
        public int x;
        public int y;
        public int z;

        public IntVector3(Vector3 vector3)
        {
            x = (int) vector3.x;
            y = (int) vector3.y;
            z = (int) vector3.z;
        }

        public IntVector3(float x, float y, float z)
        {
            this.x = (int) x;
            this.y = (int) y;
            this.z = (int) z;
        }

        public Vector3 ToUnityVector3()
        {
            return new Vector3(x, y, z);
        }

        public float Distance(IntVector3 v)
        {
            return Mathf.Sqrt(DistanceSqure(v));
        } 
        
        public float DistanceSqure(IntVector3 v)
        {
            return Mathf.Pow(v.x - x, 2) + Mathf.Pow(v.y - y, 2) + Mathf.Pow(v.z - z, 2);
        }
        
        public float PlaneDistanceSqure(IntVector3 v)
        {
            return Mathf.Pow(v.x - x, 2) + Mathf.Pow(v.z - z, 2);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }

        public static IntVector3 NewWithFloorToChunkGridCoord(Vector3 v)
        {
            return new IntVector3(MathHelper.FloorInt(v.x / 16), 0, MathHelper.FloorInt(v.z / 16));
        }
    }
}