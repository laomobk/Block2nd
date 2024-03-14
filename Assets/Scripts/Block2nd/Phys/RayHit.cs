using Block2nd.MathUtil;
using UnityEngine;

namespace Block2nd.Phys
{
    public static class RayHitNormalDirection
    {
        public static readonly byte Forward = 1;
        public static readonly byte Back = 2;
        public static readonly byte Left = 3;
        public static readonly byte Right = 4;
        public static readonly byte Up = 5;
        public static readonly byte Down = 6;
    }
    
    public class RayHit
    {
        public static readonly Vector3[] Normals = new []
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(-1, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0)
        };

        public byte normalDirection;
        public Vector3 point;
        public int blockX, blockY, blockZ;
        
        /// <summary>
        ///     代表一个在方块上移动的点
        /// </summary>
        /// <param name="x">方块的 X 坐标</param>
        /// <param name="y">方块的 Y 坐标</param>
        /// <param name="z">方块的 Z 坐标</param>
        /// <param name="normalDirection">法线朝向: 1 ~ 6 分别为 forward, back, left, right, up, down, 0 表示动点在碰撞盒中</param>
        /// <param name="hitPoint">动点</param>
        public RayHit(int x, int y, int z, byte normalDirection, Vector3 point)
        {
            this.normalDirection = normalDirection;
            this.point = point;
            blockX = x;
            blockY = y;
            blockZ = z;
        }

        public IntVector3 ToIntVector3()
        {
            return new IntVector3(blockX, blockY, blockZ);
        }

        public IntVector3 ToNormalAlongIntVector3()
        {
            var norm = Normals[normalDirection];
            return new IntVector3(blockX + (int) norm.x, blockY + (int) norm.y, blockZ + (int) norm.z);
        }
    }
}