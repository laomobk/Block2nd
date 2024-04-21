using UnityEngine;

namespace Block2nd.Model
{
    public static class QuadMeshHelper
    {
        public static void AppendQuadVertices(
            ref byte posIdx, ref byte texcoordIdx, ref byte triangleIdx,
            Vector3 origin, Vector3 up, Vector3 right, 
            float texX, float texY, float texWidth, float texHeight,
            Vector3[] tempPositionsArray, Vector2[] tempTexcoordsArray, int[] tempTrianglesArray)
        {
            int startIdx = posIdx;
            
            tempPositionsArray[posIdx++] = origin;
            tempPositionsArray[posIdx++] = origin + up;
            tempPositionsArray[posIdx++] = origin + right;
            tempPositionsArray[posIdx++] = origin + up + right;
            
            tempTexcoordsArray[texcoordIdx++] = new Vector2(texX, texY);
            tempTexcoordsArray[texcoordIdx++] = new Vector2(texX, texY + texHeight);
            tempTexcoordsArray[texcoordIdx++] = new Vector2(texX + texWidth, texY);
            tempTexcoordsArray[texcoordIdx++] = new Vector2(texX + texWidth, texY + texHeight);

            tempTrianglesArray[triangleIdx++] = startIdx + 0;
            tempTrianglesArray[triangleIdx++] = startIdx + 1;
            tempTrianglesArray[triangleIdx++] = startIdx + 2;

            tempTrianglesArray[triangleIdx++] = startIdx + 2;
            tempTrianglesArray[triangleIdx++] = startIdx + 1;
            tempTrianglesArray[triangleIdx++] = startIdx + 3;
        }
    }
}