using System;
using System.Runtime.CompilerServices;
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
            x = Mathf.FloorToInt(vector3.x);
            y = Mathf.FloorToInt(vector3.y);
            z = Mathf.FloorToInt(vector3.z);
        }

        public IntVector3(float x, float y, float z)
        {
            this.x = (int) x;
            this.y = (int) y;
            this.z = (int) z;
        }

        public IntVector3(float x, float y)
        {
            this.x = (int) x;
            this.y = (int) y;
            this.z = 0;
        }


        public IntVector3(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
        }

        public IntVector3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToUnityVector3()
        {
            return new Vector3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(IntVector3 v)
        {
            return Mathf.Sqrt(DistanceSqure(v));
        } 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DistanceSqure(IntVector3 v)
        {
            return Mathf.Pow(v.x - x, 2) + Mathf.Pow(v.y - y, 2) + Mathf.Pow(v.z - z, 2);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float PlaneDistanceSqure(IntVector3 v)
        {
            return Mathf.Pow(v.x - x, 2) + Mathf.Pow(v.z - z, 2);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector3 NewWithFloorToChunkGridCoord(Vector3 v)
        {
            return new IntVector3(Mathf.FloorToInt(v.x / 16), 0, Mathf.FloorToInt(v.z / 16));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntVector3 ToChunkCoordPos()
        {
            return new IntVector3(x >> 4, y, z >> 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntVector3 FromFloorVector3(Vector3 v)
        {
            return new IntVector3(
                Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        }

        public void Deconstruct(out int x, out int y, out int z)
        {
            x = this.x;
            y = this.y;
            z = this.z;
        }
    }
}