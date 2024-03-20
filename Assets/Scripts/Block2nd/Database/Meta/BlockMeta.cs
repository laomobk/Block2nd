using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Behavior;
using Block2nd.Phys;
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

        public byte positionCount;
        public byte triangleCount;
        public byte texcoordCount;
        public byte colorsCount;
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

        public bool liquid;
        public bool transparent;
        public bool forceRenderAllFace;

        public byte initState;
        
        public BlockBehavior behavior = new StaticBlockBehavior();
        public AABB aabb = AABB.One();
        
        [NotNull] public BlockShape shape;
    }
}