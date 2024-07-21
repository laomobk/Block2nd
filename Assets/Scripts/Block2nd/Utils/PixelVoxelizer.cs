using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.Utils
{
    public struct VoxelizeTask
    {
        public Texture2D targetTexture;
        public Vector2 uvOrigin;
        public float width;
        public float height;

        public VoxelizeTask(Texture2D targetTexture, Vector2 uvOrigin, float width, float height)
        {
            this.targetTexture = targetTexture;
            this.uvOrigin = uvOrigin;
            this.width = width;
            this.height = height;
        }
    }
    
    public class PixelVoxelizer
    {
        public static BlockMesh Do(VoxelizeTask task, float depth = 0.05f)
        {
            var meshBuilder = new MeshBuilder();

            var taskUvOrigin = task.uvOrigin;
            var taskUvRight = Vector2.right * task.width;
            var taskUvUp = Vector2.up * task.height;
            
            var texture = task.targetTexture;

            var texHeight = texture.height;
            var texWidth = texture.width;

            var uvTexelWidth = 1f / texWidth;
            var uvTexelHeight = 1f / texHeight;

            var taskWidth = (int) (texWidth * task.width);
            var taskHeight = (int) (texHeight * task.height);

            var texelWidth = 1f / taskWidth;
            var texelHeight = 1f / taskHeight;
            
            meshBuilder.SetQuadUV(taskUvOrigin, task.width, task.height);
            meshBuilder.AddQuad(Vector3.zero, Vector3.right, Vector3.up, 1, 1);
            meshBuilder.AddQuad(Vector3.zero + Vector3.forward * depth, Vector3.right, Vector3.up, 1, 1, false);

            var taskBaseX = (int) (texWidth * taskUvOrigin.x);
            var taskBaseY = (int) (texHeight * taskUvOrigin.y);

            for (int x = -1; x < taskWidth; ++x)
            {
                for (int y = -1; y < taskHeight; ++y)
                {
                    var pixelColor = GetPixel(texture, taskBaseX + x, taskBaseY + y, 
                        taskWidth, taskHeight, taskBaseX, taskBaseY);
                    
                    if (pixelColor.a < 0.1f)
                    {
                        var colorNeighbor = GetPixel(texture, taskBaseX + x, taskBaseY + y + 1, taskWidth, taskHeight, 
                            taskBaseX, taskBaseY);
                        if (colorNeighbor.a > 0.1f)
                        {
                            meshBuilder.SetQuadUV(taskUvOrigin + 
                                                  taskUvUp * ((1f + y) / taskHeight) + 
                                                  taskUvRight * ((float) x / taskWidth), 
                                uvTexelWidth, uvTexelHeight);
                            AddUpFace(meshBuilder, x * texelWidth, y * texelHeight,
                                        texelWidth, texelHeight, depth, false);
                        }
                        colorNeighbor = GetPixel(texture, taskBaseX + x + 1, taskBaseY + y, taskWidth, taskHeight, 
                            taskBaseX, taskBaseY);
                        if (colorNeighbor.a > 0.1f)
                        {
                            meshBuilder.SetQuadUV(taskUvOrigin + 
                                                  taskUvUp * ((float) y / taskHeight) + 
                                                  taskUvRight * ((1f + x) / taskWidth), 
                                uvTexelWidth, uvTexelHeight);
                            AddRightFace(meshBuilder, x * texelWidth, y * texelHeight,
                                texelWidth, texelHeight, depth);
                        }
                    }
                    else
                    {
                        var colorNeighbor = GetPixel(texture, taskBaseX + x, taskBaseY + y + 1, taskWidth, taskHeight, 
                            taskBaseX, taskBaseY);
                        if (colorNeighbor.a < 0.1f)
                        {
                            meshBuilder.SetQuadUV(taskUvOrigin + 
                                                  taskUvUp * ((float) y / taskHeight) + 
                                                  taskUvRight * ((float) x / taskWidth), 
                                uvTexelWidth, uvTexelHeight);
                            AddUpFace(meshBuilder, x * texelWidth, y * texelHeight,
                                texelWidth, texelHeight, depth);
                        }
                        colorNeighbor = GetPixel(texture, taskBaseX + x + 1, taskBaseY + y, taskWidth, taskHeight, 
                            taskBaseX, taskBaseY);
                        if (colorNeighbor.a < 0.1f)
                        {
                            meshBuilder.SetQuadUV(taskUvOrigin + 
                                                  taskUvUp * ((float) y / taskHeight) + 
                                                  taskUvRight * ((float) x / taskWidth), 
                                uvTexelWidth, uvTexelHeight);
                            AddRightFace(meshBuilder, x * texelWidth, y * texelHeight,
                                texelWidth, texelHeight, depth, false);
                        }
                    }
                }
            }
            
            return meshBuilder.GetBlockMesh();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color GetPixel(Texture2D texture2D, int x, int y, int tW, int tH, int bX, int bY)
        {
            if (x < bX || y < bY || x >= bX + tW || y >= bY + tH)
                return Color.clear;
            return texture2D.GetPixel(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddUpFace(MeshBuilder meshBuilder, 
            float originX, float originY, float tW, float tH, float depth, bool invert = true)
        {
            meshBuilder.AddQuad(new Vector3(originX, originY + tH), 
                Vector3.right, Vector3.forward, tW, depth, invert);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddRightFace(MeshBuilder meshBuilder, 
            float originX, float originY, float tW, float tH, float depth, bool invert = true)
        {
            meshBuilder.AddQuad(
                new Vector3(originX + tW, originY, depth), 
                                  Vector3.back, Vector3.up, depth, tH, invert);
        }
    }
}