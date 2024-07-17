using System;
using UnityEngine;

namespace Block2nd.Database.Meta
{
    [Serializable]
    public class CubeBlockShape : BlockShape
    {
        public static CubeBlockShape Default = new CubeBlockShape(new CubeAppearance());

        [Serializable]
        public struct CubeAppearance
        {
            public int frontTexIdx;
            public int backTexIdx;
            public int leftTexIdx;
            public int rightTexIdx;
            public int topTexIdx;
            public int bottomTexIdx;
            public static CubeAppearance New(
                int frontIdx, int backIdx, int leftIdx, int rightIdx, int topIdx, int bottomIdx)
            {
                return new CubeAppearance
                {
                    frontTexIdx = frontIdx,
                    backTexIdx = backIdx,
                    leftTexIdx = leftIdx,
                    rightTexIdx = rightIdx,
                    topTexIdx = topIdx,
                    bottomTexIdx = bottomIdx
                };
            }

            public static CubeAppearance NewSameFace(int faceIdx)
            {
                return new CubeAppearance
                {
                    frontTexIdx = faceIdx,
                    backTexIdx = faceIdx,
                    leftTexIdx = faceIdx,
                    rightTexIdx = faceIdx,
                    topTexIdx = faceIdx,
                    bottomTexIdx = faceIdx
                };
            }
        }

        // pooling memory...
        private Vector3[] tempPositionsArray;
        private Vector2[] tempTexcoordsArray;
        private Color[] tempColorsArray;
        private int[] tempTrianglesArray;

        protected float width;
        protected float depth;
        protected float height;
        
        protected Vector3 original;
        protected Vector3 forward;
        protected Vector3 up;
        protected Vector3 right;

        protected Vector3 BBR;
        protected Vector3 BTR;
        protected Vector3 BBL;
        protected Vector3 BTL;
        protected Vector3 FBR;
        protected Vector3 FBL;
        protected Vector3 FTR;
        protected Vector3 FTL;

        private static Mesh _cubeMesh;

        public CubeAppearance appearance;
        
        public CubeBlockShape(CubeAppearance appearance, 
                                float width = 1f, float depth = 1f, float height = 1f,
                                float ofsX = 0f, float ofsY = 0f, float ofsZ = 0f)
        {
            this.appearance = appearance;

            tempPositionsArray = new Vector3[24];
            tempTexcoordsArray = new Vector2[24];
            tempColorsArray = new Color[24];
            tempTrianglesArray = new int[36];

            this.width = width;
            this.depth = depth;
            this.height = height;
            
            original = new Vector3(ofsX, ofsY, ofsZ);

            RecalculateVertexVectors();
        }

        protected void RecalculateVertexVectors()
        {
            forward = new Vector3(0, 0, depth);
            up = new Vector3(0, height, 0);
            right = new Vector3(width, 0, 0);
            
            BBR = original;  // B(ottom) B(ack) R(ight)
            BBL = original + right;
            BTR = original + up;
            BTL = original + up + right;
            
            FBR = original + forward;
            FBL = FBR + right;
            FTR = FBR + up;
            FTL = FBR + up + right;
        }

        public static CubeBlockShape NewWithTexIdx(
            int frontIdx, int backIdx, int leftIdx, int rightIdx, int topIdx, int bottomIdx)
        {
            return new CubeBlockShape(CubeAppearance.New(
                frontIdx, backIdx, leftIdx, rightIdx, topIdx, bottomIdx));
        }

        private void _BuildFace(
            ref byte posIdx, ref byte texcoordIdx, ref byte triangleIdx,
            Vector3 origin, Vector3 up, Vector3 right, 
            Vector2 texcoordOrigin, Vector2 texcoordUp, Vector2 texcoordRight, bool flip)
        {
            int startIdx = posIdx;
            
            tempPositionsArray[posIdx++] = origin;
            tempPositionsArray[posIdx++] = origin + up;
            tempPositionsArray[posIdx++] = origin + right;
            tempPositionsArray[posIdx++] = origin + up + right;
            
            tempTexcoordsArray[texcoordIdx++] = texcoordOrigin;
            tempTexcoordsArray[texcoordIdx++] = texcoordOrigin + texcoordUp;
            tempTexcoordsArray[texcoordIdx++] = texcoordOrigin + texcoordRight;
            tempTexcoordsArray[texcoordIdx++] = texcoordOrigin + texcoordRight + texcoordUp;

            if (flip)
            {
                tempTrianglesArray[triangleIdx++] = startIdx + 0;
                tempTrianglesArray[triangleIdx++] = startIdx + 3;
                tempTrianglesArray[triangleIdx++] = startIdx + 2;

                tempTrianglesArray[triangleIdx++] = startIdx + 0;
                tempTrianglesArray[triangleIdx++] = startIdx + 1;
                tempTrianglesArray[triangleIdx++] = startIdx + 3;
            }
            else
            {
                tempTrianglesArray[triangleIdx++] = startIdx + 0;
                tempTrianglesArray[triangleIdx++] = startIdx + 1;
                tempTrianglesArray[triangleIdx++] = startIdx + 2;

                tempTrianglesArray[triangleIdx++] = startIdx + 2;
                tempTrianglesArray[triangleIdx++] = startIdx + 1;
                tempTrianglesArray[triangleIdx++] = startIdx + 3;
            }
        }

        public override BlockMesh GetShapeMesh(int exposedFace, long lightAttenuation, int aoBits = 0)
        {
            byte posIdx = 0, colorsIdx = 0, texcoordsIdx = 0, trianglesIdx = 0;

            var uvUp = new Vector2(0, AtlasTextureDescriptor.Default.VStep);
            var uvRight = new Vector2(AtlasTextureDescriptor.Default.UStep, 0);
            
            if ((exposedFace & 1) != 0)  // front 
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.frontTexIdx);

                var color = new Color(
                    (lightAttenuation & 15) / 15f, ((lightAttenuation >> 24) & 15) / 15f, (aoBits >> 6) & 1);
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 4) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 7) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 5) & 1;
                tempColorsArray[colorsIdx++] = color;

                bool flip = false;
                if (aoBits > 0)
                {
                    flip = tempColorsArray[colorsIdx - 3].b + tempColorsArray[colorsIdx - 2].b >
                           tempColorsArray[colorsIdx - 4].b + tempColorsArray[colorsIdx - 1].b;
                }

                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    FBL, up, -right, texcoordOrigin, uvUp, uvRight, flip);
            }

            if ((exposedFace & 2) != 0)  // back
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.backTexIdx);

                var color = new Color(
                    ((lightAttenuation >> 4) & 15) / 15f, ((lightAttenuation >> 28) & 15) / 15f, (aoBits >> 3) & 1);
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 1) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 2) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = aoBits & 1;
                tempColorsArray[colorsIdx++] = color;

                bool flip = false;
                if (aoBits > 0)
                {
                    flip = tempColorsArray[colorsIdx - 3].b + tempColorsArray[colorsIdx - 2].b >
                           tempColorsArray[colorsIdx - 4].b + tempColorsArray[colorsIdx - 1].b;
                }

                
                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    BBR, up, right, texcoordOrigin, uvUp, uvRight, flip);
            }
            
            if ((exposedFace & 4) != 0)  // left
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.leftTexIdx);

                var color = new Color(
                    ((lightAttenuation >> 8) & 15) / 15f, ((lightAttenuation >> 32) & 15) / 15f, (aoBits >> 7) & 1);
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 5) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 3) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 1) & 1;
                tempColorsArray[colorsIdx++] = color;

                bool flip = false;
                if (aoBits > 0)
                {
                    flip = tempColorsArray[colorsIdx - 3].b + tempColorsArray[colorsIdx - 2].b >
                           tempColorsArray[colorsIdx - 4].b + tempColorsArray[colorsIdx - 1].b;
                }


                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    FBR, up, -forward, texcoordOrigin, uvUp, uvRight, flip);
            }
            
            if ((exposedFace & 8) != 0)  // right
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.rightTexIdx);

                var color = new Color(
                    ((lightAttenuation >> 12) & 15) / 15f, ((lightAttenuation >> 36) & 15) / 15f, (aoBits >> 2) & 1);
                tempColorsArray[colorsIdx++] = color;
                color.b = aoBits & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 6) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 4) & 1;
                tempColorsArray[colorsIdx++] = color;

                bool flip = false;
                if (aoBits > 0)
                {
                    flip = tempColorsArray[colorsIdx - 3].b + tempColorsArray[colorsIdx - 2].b >
                           tempColorsArray[colorsIdx - 4].b + tempColorsArray[colorsIdx - 1].b;
                }

                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    BBL, up, forward, texcoordOrigin, uvUp, uvRight, flip);
            }
            
            if ((exposedFace & 16) != 0)  // top
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.topTexIdx);

                var color = new Color(
                    ((lightAttenuation >> 16) & 15) / 15f, ((lightAttenuation >> 40) & 15) / 15f, (aoBits >> 4) & 1);
                tempColorsArray[colorsIdx++] = color;
                color.b = aoBits & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 5) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 1) & 1;
                tempColorsArray[colorsIdx++] = color;

                bool flip = false;
                if (aoBits > 0)
                {
                    flip = tempColorsArray[colorsIdx - 3].b + tempColorsArray[colorsIdx - 2].b >
                           tempColorsArray[colorsIdx - 4].b + tempColorsArray[colorsIdx - 1].b;
                }

                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    FTL, -forward, -right, texcoordOrigin, uvUp, uvRight, flip);
            }
            
            if ((exposedFace & 32) != 0)  // bottom
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.bottomTexIdx);

                var color = new Color(
                    ((lightAttenuation >> 20) & 15) / 15f, ((lightAttenuation >> 44) & 15) / 15f, (aoBits >> 2) & 1);
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 6) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 3) & 1;
                tempColorsArray[colorsIdx++] = color;
                color.b = (aoBits >> 7) & 1;
                tempColorsArray[colorsIdx++] = color;

                bool flip = false;
                if (aoBits > 0)
                {
                    flip = tempColorsArray[colorsIdx - 3].b + tempColorsArray[colorsIdx - 2].b >
                           tempColorsArray[colorsIdx - 4].b + tempColorsArray[colorsIdx - 1].b;
                }

                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    BBL, forward, -right, texcoordOrigin, uvUp, uvRight, flip);
            }

            var verts = new BlockMesh
            {
                positions = tempPositionsArray,
                triangles = tempTrianglesArray,
                texcoords = tempTexcoordsArray,
                colors = tempColorsArray,
                
                positionCount = posIdx,
                colorsCount = colorsIdx,
                texcoordCount = texcoordsIdx,
                triangleCount = trianglesIdx
            };

            return verts;
        }
    }
}