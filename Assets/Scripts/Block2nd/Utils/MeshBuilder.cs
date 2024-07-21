using System.Collections.Generic;
using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.Utils
{
    public class MeshBuilder
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> indices = new List<int>();
        public List<Vector2> texcoords = new List<Vector2>();
        public List<Color> colors = new List<Color>();

        protected Vector2 QuadVertex0UV;
        protected Vector2 QuadVertex1UV;
        protected Vector2 QuadVertex2UV;
        protected Vector2 QuadVertex3UV;
        
        protected Color QuadVertex0Color;
        protected Color QuadVertex1Color;
        protected Color QuadVertex2Color;
        protected Color QuadVertex3Color;

        public void SetQuadUV(Vector2 origin, float width, float height)
        {
            var right = Vector2.right * width;
            var up = Vector2.up * height;
            QuadVertex0UV = origin;
            QuadVertex1UV = origin + right;
            QuadVertex2UV = origin + up;
            QuadVertex3UV = origin + right + up;
        }

        public void SetQuadColor(Color color0, Color color1, Color color2, Color color3)
        {
            QuadVertex0Color = color0;
            QuadVertex1Color = color1;
            QuadVertex2Color = color2;
            QuadVertex3Color = color3;
        }

        /// <summary>
        ///   2       3
        ///   +-------+
        ///   |   /   |
        ///   |  /    |
        ///   +-------+
        ///   0       1
        /// </summary>
        public void AddQuad(Vector3 origin, Vector3 right, Vector3 up, float width, float height, bool invert = true)
        {
            up.Normalize();
            right.Normalize();
            
            var rightVec = right * width;
            var upVec = up * height;

            var startIdx = vertices.Count;
            vertices.Add(origin);
            vertices.Add(origin + rightVec);
            vertices.Add(origin + upVec);
            vertices.Add(origin + rightVec + upVec);

            if (invert)
            {
                indices.Add(startIdx + 0);
                indices.Add(startIdx + 2);
                indices.Add(startIdx + 3);
                indices.Add(startIdx + 0);
                indices.Add(startIdx + 3);
                indices.Add(startIdx + 1);
            }
            else
            {
                indices.Add(startIdx + 0);
                indices.Add(startIdx + 3);
                indices.Add(startIdx + 2);
                indices.Add(startIdx + 0);
                indices.Add(startIdx + 1);
                indices.Add(startIdx + 3);
            }

            texcoords.Add(QuadVertex0UV);
            texcoords.Add(QuadVertex1UV);
            texcoords.Add(QuadVertex2UV);
            texcoords.Add(QuadVertex3UV);
            
            colors.Add(QuadVertex0Color);
            colors.Add(QuadVertex1Color);
            colors.Add(QuadVertex2Color);
            colors.Add(QuadVertex3Color);
        }

        public BlockMesh GetBlockMesh()
        {
            return new BlockMesh
            {
                positions = vertices.ToArray(),
                colors = colors.ToArray(),
                texcoords = texcoords.ToArray(),
                triangles = indices.ToArray(),

                positionCount = vertices.Count,
                triangleCount = indices.Count,
                colorsCount = colors.Count,
                texcoordCount = texcoords.Count,
            };
        }
    }
}