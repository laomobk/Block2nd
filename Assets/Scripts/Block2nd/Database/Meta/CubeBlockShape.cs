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
            Vector2 texcoordOrigin, Vector2 texcoordUp, Vector2 texcoordRight)
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
            
            tempTrianglesArray[triangleIdx++] = startIdx + 0;
            tempTrianglesArray[triangleIdx++] = startIdx + 3;
            tempTrianglesArray[triangleIdx++] = startIdx + 2;

            tempTrianglesArray[triangleIdx++] = startIdx + 0;
            tempTrianglesArray[triangleIdx++] = startIdx + 1;
            tempTrianglesArray[triangleIdx++] = startIdx + 3;
        }

        public override BlockMesh GetShapeMesh(int exposedFace, int lightAttenuation)
        {
            byte posIdx = 0, colorsIdx = 0, texcoordsIdx = 0, trianglesIdx = 0;

            var uvUp = new Vector2(0, AtlasTextureDescriptor.Default.VStep);
            var uvRight = new Vector2(AtlasTextureDescriptor.Default.UStep, 0);
            
            if ((exposedFace & 1) != 0)  // front 
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.frontTexIdx);
                
                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    FBL, up, -right, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 1) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
            }

            if ((exposedFace & 2) != 0)  // back
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.backTexIdx);

                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    BBR, up, right, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 2) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
            }
            
            if ((exposedFace & 4) != 0)  // left
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.leftTexIdx);

                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    FBR, up, -forward, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 4) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
            }
            
            if ((exposedFace & 8) != 0)  // right
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.rightTexIdx);

                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    BBL, up, forward, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 8) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
            }
            
            if ((exposedFace & 16) != 0)  // top
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.topTexIdx);

                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    FTL, -forward, -right, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 16) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
            }
            
            if ((exposedFace & 32) != 0)  // bottom
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.bottomTexIdx);

                _BuildFace(
                    ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                    BBL, forward, -right, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 32) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
                tempColorsArray[colorsIdx++] = color;
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