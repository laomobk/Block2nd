using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Database.Meta
{
    public class PlantShape: BlockShape
    {
        private static Vector3[] _crossPlaneVertices = new[]
        {
            new Vector3(0.1f, 0.0f, 0.1f),
            new Vector3(0.1f, 0.9f, 0.1f),
            new Vector3(0.9f, 0.9f, 0.9f),
            new Vector3(0.9f, 0, 0.9f),
            
            new Vector3(0.9f, 0, 0.1f),
            new Vector3(0.9f, 0.9f, 0.1f),
            new Vector3(0.1f, 0.9f, 0.9f),
            new Vector3(0.1f, 0, 0.9f),
        };

        private static int[] _crossPlaneIndices = new[]
        {
            0, 1, 2,
            0, 2, 3,
            4, 5, 6,
            4, 6, 7
        };

        private static Vector2[] _uvs = new Vector2[8];
        private static Color[] _colors = new Color[8];
        

        private static Vector2 _uvUp = new Vector2(0, AtlasTextureDescriptor.Default.VStep);
        private static Vector2 _uvRight = new Vector2(AtlasTextureDescriptor.Default.UStep, 0);

        private int texIdx;

        public PlantShape(int texIdx)
        {
            this.texIdx = texIdx;
        }

        public override BlockMesh GetShapeMesh(int exposedFace, long lightAttenuation, int aoBits = 0)
        {
            var uvOriginal = AtlasTextureDescriptor.Default.GetUVByIndex(texIdx);

            var uvUp = uvOriginal + _uvUp;
            var uvRight = uvOriginal + _uvRight;
            var uvRT = uvOriginal + _uvUp + _uvRight;

            _uvs[0] = uvOriginal;
            _uvs[1] = uvUp;
            _uvs[2] = uvRT;
            _uvs[3] = uvRight;
            
            _uvs[4] = uvOriginal;
            _uvs[5] = uvUp;
            _uvs[6] = uvRT;
            _uvs[7] = uvRight;
            
            var color = new Color(((lightAttenuation >> 16) & 15) / 15f, ((lightAttenuation >> 40) & 15) / 15f, (aoBits >> 4));
            _colors[0] = color;
            _colors[1] = color;
            _colors[2] = color;
            _colors[3] = color;
            _colors[4] = color;
            _colors[5] = color;
            _colors[6] = color;
            _colors[7] = color;

            return new BlockMesh
            {
                colors = _colors,
                positions = _crossPlaneVertices,
                texcoords = _uvs,
                triangles = _crossPlaneIndices,

                colorsCount = 8,
                positionCount = 8,
                texcoordCount = 8,
                triangleCount = 12
            };
        }
    }
}