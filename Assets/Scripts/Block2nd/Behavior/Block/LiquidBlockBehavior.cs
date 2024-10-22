﻿using Block2nd.Database;
using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Behavior.Block
{
    public abstract class LiquidBlockBehavior : BlockBehavior
    {
        public static readonly byte DefaultIterCount = 8;
        
        protected abstract int GetSelfBlockCode();

        public override void OnAfterPlace(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            level.SetBlockState(worldPos.x, worldPos.y, worldPos.z, DefaultIterCount, false);
            
            OnUpdate(worldPos, level, chunk, player);
        }

        public override void OnInit(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
        }

        public override void OnUpdate(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            byte state = level.GetBlock(worldPos.x, worldPos.y, worldPos.z).blockState;
            byte iterCount = (byte) (state & 15);

            var x = worldPos.x;
            var y = worldPos.y;
            var z = worldPos.z;

            Chunk cp;

            var belowBlock = level.GetBlock(x, y - 1, z, out cp);

            if (belowBlock.blockCode == GetSelfBlockCode())
            {
                return;
            }
            
            if (y > 0 && CanBeWashed(belowBlock.blockCode))
            {
                if (belowBlock.blockCode != 0)
                {
                    level.DestroyBlock(x, y - 1, z);
                }
                
                level.SetBlock(
                    GetSelfBlockCode(), x, y - 1, z, false, state: iterCount, useInitState: false);

                level.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x, y - 1, z),
                    onlyUpdateCenterBlock = true
                });
                
                return;
            }

            if (iterCount == 0)
            {
                return;
            }

            byte newState = (byte) (iterCount - 1);

            var code = level.GetBlock(x, y, z + 1, out cp).blockCode;
            if (CanBeWashed(code))
            {
                if (code != 0)
                {
                    level.DestroyBlock(x, y, z + 1);
                }
                
                level.SetBlock(
                    GetSelfBlockCode(), x, y, z + 1, false, state: newState, useInitState: false);

                level.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x, y, z + 1),
                    onlyUpdateCenterBlock = true
                });
            }

            code = level.GetBlock(x, y, z - 1, out cp).blockCode;
            if (CanBeWashed(code))
            {
                if (code != 0)
                {
                    level.DestroyBlock(x, y, z - 1);
                }
                level.SetBlock(
                    GetSelfBlockCode(), x, y, z - 1, false, state: newState, useInitState: false);

                level.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x, y, z - 1),
                    onlyUpdateCenterBlock = true
                });
            }

            code = level.GetBlock(x + 1, y, z, out cp).blockCode;
            if (CanBeWashed(code))
            {
                if (code != 0)
                {
                    level.DestroyBlock(x + 1, y, z);
                }
                level.SetBlock(
                    GetSelfBlockCode(), x + 1, y, z, false, state: newState, useInitState: false);

                level.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x + 1, y, z),
                    onlyUpdateCenterBlock = true
                });
            }

            code = level.GetBlock(x - 1, y, z, out cp).blockCode;
            if (CanBeWashed(code))
            {
                if (code != 0)
                {
                    level.DestroyBlock(x - 1, y, z);
                }
                level.SetBlock(
                    GetSelfBlockCode(), x - 1, y, z, false, state: newState, useInitState: false);

                level.AddUpdateToNextTick(new ChunkUpdateContext
                {
                    chunk = cp,
                    pos = new IntVector3(x - 1, y, z),
                    onlyUpdateCenterBlock = true
                });
            }
        }

        protected bool CanBeWashed(int code)
        {
            var meta = BlockMetaDatabase.GetBlockMetaByCode(code);

            if (meta == null || meta.plant)
            {
                return true;
            }

            return false;
        }
    }
}