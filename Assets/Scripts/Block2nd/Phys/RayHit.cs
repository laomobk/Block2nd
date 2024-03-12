using UnityEngine;

namespace Block2nd.Phys
{
    public class RayHit
    {
        public byte direction;
        public Vector3 point;
        
        /// <summary>
        ///     代表一个在方块上移动的点
        /// </summary>
        /// <param name="x">方块的 X 坐标</param>
        /// <param name="y">方块的 Y 坐标</param>
        /// <param name="z">方块的 Z 坐标</param>
        /// <param name="direction">法线朝向: 1 ~ 6 分别为 forward, left, right, up, down, 0 表示动点在碰撞盒中</param>
        /// <param name="hitPoint">动点</param>
        public RayHit(byte direction, Vector3 point)
        {
            this.direction = direction;
            this.point = point;
        }
    }
}