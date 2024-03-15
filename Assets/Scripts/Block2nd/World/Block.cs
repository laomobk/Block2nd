using System;
using System.Collections.Generic;
using Block2nd.Behavior;
using Block2nd.Database;
using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.World
{
    public enum BlockSurface
    {
        NONE,
        FRONT,
        BACK,
        LEFT,
        RIGHT,
        TOP,
        BOTTOM,
    }

    [Serializable]
    public struct ChunkBlockData
    {
        public int blockCode;
        public byte blockState;

        public static ChunkBlockData EMPTY = new ChunkBlockData
        {
            blockCode = 0,
            blockState = 0,
        };

        public bool Transparent()
        {
            var meta = BlockMetaDatabase.GetBlockMetaByCode(blockCode);
            if (meta != null)
                return meta.transparent;
            return true;
        }
    }
}