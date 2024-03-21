using Block2nd.Phys;
using UnityEngine;

namespace Block2nd.DebugUtil
{
    public class DebugAABB
    {
        public static void DrawAABBInSceneView(AABB aabb, Color color)
        {
            Debug.DrawLine(new Vector3(aabb.minX, aabb.minY, aabb.minZ), new Vector3(aabb.maxX, aabb.minY, aabb.minZ), color, 0.1f);
            Debug.DrawLine(new Vector3(aabb.minX, aabb.maxY, aabb.minZ), new Vector3(aabb.maxX, aabb.maxY, aabb.minZ), color, 0.1f);
            
            Debug.DrawLine(new Vector3(aabb.minX, aabb.minY, aabb.maxZ), new Vector3(aabb.maxX, aabb.minY, aabb.maxZ), color, 0.1f);
            Debug.DrawLine(new Vector3(aabb.minX, aabb.maxY, aabb.maxZ), new Vector3(aabb.maxX, aabb.maxY, aabb.maxZ), color, 0.1f);
            
            Debug.DrawLine(new Vector3(aabb.minX, aabb.minY, aabb.minZ), new Vector3(aabb.minX, aabb.minY, aabb.maxZ), color, 0.1f);
            Debug.DrawLine(new Vector3(aabb.minX, aabb.maxY, aabb.minZ), new Vector3(aabb.minX, aabb.maxY, aabb.maxZ), color, 0.1f);

            Debug.DrawLine(new Vector3(aabb.maxX, aabb.minY, aabb.minZ), new Vector3(aabb.maxX, aabb.minY, aabb.maxZ), color, 0.1f);
            Debug.DrawLine(new Vector3(aabb.maxX, aabb.maxY, aabb.minZ), new Vector3(aabb.maxX, aabb.maxY, aabb.maxZ), color, 0.1f);
            
            Debug.DrawLine(new Vector3(aabb.minX, aabb.minY, aabb.minZ), new Vector3(aabb.minX, aabb.maxY, aabb.minZ), color, 0.1f);
            Debug.DrawLine(new Vector3(aabb.minX, aabb.minY, aabb.maxZ), new Vector3(aabb.minX, aabb.maxY, aabb.maxZ), color, 0.1f);
            
            Debug.DrawLine(new Vector3(aabb.maxX, aabb.minY, aabb.minZ), new Vector3(aabb.maxX, aabb.maxY, aabb.minZ), color, 0.1f);
            Debug.DrawLine(new Vector3(aabb.maxX, aabb.minY, aabb.maxZ), new Vector3(aabb.maxX, aabb.maxY, aabb.maxZ), color, 0.1f);
        }
    }
}