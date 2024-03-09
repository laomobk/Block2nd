using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Behavior;
using JetBrains.Annotations;
using UnityEngine;

namespace Block2nd.Database.Meta
{
    [Serializable]
    public class BlockMesh
    {
        public Vector3[] positions;
        public int[] triangles;
        public Vector2[] texcoords;
        public Color[] colors;
    }
    
    [Serializable]
    public abstract class BlockShape
    {
        /// <summary>
        ///     得到用于渲染的网格信息
        /// </summary>
        /// <param name="exposedFace">
        ///     8 比特数，低到高位依次代表面：前 后 左 右 上 下 被暴露。
        /// </param>
        /// <param name="lightAttenuation">
        ///     8 比特数，低到高位依次代表面：前 后 左 右 上 下 是否发生管线衰减。
        /// </param>
        /// <returns></returns>
        public abstract BlockMesh GetShapeMesh(int exposedFace, int lightAttenuation);

        public Vector3 GetCenterPoint()
        {
            var shapeMash = GetShapeMesh(255, 0);
            var center = new Vector3(0, 0, 0);

            foreach (var vert in shapeMash.positions)
            {
                center += vert;
            }

            center /= shapeMash.positions.Length;

            return center;
        }
    }

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
        
        private static Mesh _cubeMesh;

        public CubeAppearance appearance;
        
        public CubeBlockShape(CubeAppearance appearance)
        {
            this.appearance = appearance;
        }

        public static CubeBlockShape NewWithTexIdx(
            int frontIdx, int backIdx, int leftIdx, int rightIdx, int topIdx, int bottomIdx)
        {
            return new CubeBlockShape(CubeAppearance.New(
                frontIdx, backIdx, leftIdx, rightIdx, topIdx, bottomIdx));
        }

        private void _BuildFace(List<Vector3> vertices, List<int> triangles, List<Vector2> texcoords,
            Vector3 origin, Vector3 up, Vector3 right, 
            Vector2 texcoordOrigin, Vector2 texcoordUp, Vector2 texcoordRight)
        {
            var startIdx = vertices.Count;
            
            vertices.Add(origin);
            vertices.Add(origin + up);
            vertices.Add(origin + right);
            vertices.Add(origin + up + right);
            
            texcoords.Add(texcoordOrigin);
            texcoords.Add(texcoordOrigin + texcoordUp);
            texcoords.Add(texcoordOrigin + texcoordRight);
            texcoords.Add(texcoordOrigin + texcoordRight + texcoordUp);
            
            triangles.Add(startIdx + 0);
            triangles.Add(startIdx + 3);
            triangles.Add(startIdx + 2);

            triangles.Add(startIdx + 0);
            triangles.Add(startIdx + 1);
            triangles.Add(startIdx + 3);
        }

        public override BlockMesh GetShapeMesh(int exposedFace, int lightAttenuation)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var texcoords = new List<Vector2>();
            var colors = new List<Color>();

            var uvUp = new Vector2(0, AtlasTextureDescriptor.Default.VStep);
            var uvRight = new Vector2(AtlasTextureDescriptor.Default.UStep, 0);

            if ((exposedFace & 1) != 0)  // front 
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.frontTexIdx);
                
                _BuildFace(
                    vertices, triangles, texcoords,
                    new Vector3(1, 0, 1),
                    Vector3.up, Vector3.left, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 1) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
            }

            if ((exposedFace & 2) != 0)  // back
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.backTexIdx);

                _BuildFace(
                    vertices, triangles, texcoords,
                    Vector3.zero, 
                    Vector3.up, Vector3.right, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 2) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
            }
            
            if ((exposedFace & 4) != 0)  // left
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.leftTexIdx);

                _BuildFace(
                    vertices, triangles, texcoords,
                    new Vector3(0, 0, 1), 
                    Vector3.up, Vector3.back, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 4) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
            }
            
            if ((exposedFace & 8) != 0)  // right
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.rightTexIdx);

                _BuildFace(
                    vertices, triangles, texcoords,
                    new Vector3(1, 0, 0), 
                    Vector3.up, Vector3.forward, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 8) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
            }
            
            if ((exposedFace & 16) != 0)  // top
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.topTexIdx);

                _BuildFace(
                    vertices, triangles, texcoords,
                    new Vector3(1, 1, 1), 
                    Vector3.back, Vector3.left, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 16) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
            }
            
            if ((exposedFace & 32) != 0)  // bottom
            {
                var texcoordOrigin = AtlasTextureDescriptor.Default.GetUVByIndex(appearance.bottomTexIdx);

                _BuildFace(
                    vertices, triangles, texcoords,
                    new Vector3(1, 0, 0), 
                    Vector3.forward, Vector3.left, texcoordOrigin, uvUp, uvRight);

                var color = new Color(1, 1, 1);
                if ((lightAttenuation & 32) != 0)
                {
                    color = new Color(0.7f, 0.7f, 0.7f);
                }
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
            }

            var verts = new BlockMesh();
            
            verts.positions = vertices.ToArray();
            verts.triangles = triangles.ToArray();
            verts.texcoords = texcoords.ToArray();
            verts.colors = colors.ToArray();

            return verts;
        }
    }

    [Serializable]
    public class MeshBlockShape : BlockShape
    {
        public Mesh mesh;

        public override BlockMesh GetShapeMesh(int exposedFace, int lightAttuation)
        {
            return new BlockMesh
            {
                positions = mesh.vertices,
                triangles = mesh.triangles
            };
        }
    }

    [Serializable]
    public class BlockMeta
    {
        public int blockCode = 0;
        public string blockId;
        public string blockName;
        public bool transparent;
        public BlockBehavior behavior = new StaticBlockBehavior();

        [NotNull] public BlockShape shape;
    }
}