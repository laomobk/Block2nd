using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Transparent()
        {
            var meta = BlockMetaDatabase.GetBlockMetaByCode(blockCode);
            if (meta != null)
                return meta.transparent;
            return true;  // air block is transparent.
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSolid()
        {
            return blockCode != 0 && (BlockMetaDatabase.types[blockCode] & BlockTypeBits.PlantBit) == 0;
        }

        public override string ToString()
        {
            return blockCode.ToString();
        }
    }
}