﻿using Block2nd.Audio;
using Block2nd.Database.Meta;
using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.Phys;
using Block2nd.World;

namespace Block2nd.Behavior
{
    public abstract class BlockBehavior
    {
        protected AABB aabb = AABB.LazyOneBox;
        protected BlockSoundDescriptor soundDescriptor = BlockSoundDescriptor.SoundDigGrass;

        public BlockSoundDescriptor SoundDescriptor => soundDescriptor;

        public BlockBehavior(BlockSoundDescriptor soundDescriptor = null)
        {
            if (soundDescriptor != null)
            {
                this.soundDescriptor = soundDescriptor;
            }
        }
        
        public abstract BlockBehavior CreateInstance();

        public virtual void OnInit(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            
        }

        public virtual bool OnDestroy(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            level.PlaySoundAt(soundDescriptor.breakSoundGroup.GetPath(),
                                worldPos.x, worldPos.y, worldPos.z);
            
            return true;
        }

        public virtual void OnTick(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            
        }

        public virtual bool CanRaycast()
        {
            return true;
        }

        public virtual bool CanCollide()
        {
            return true;
        }

        public AABB GetAABB(IntVector3 worldPos)
        {
            return GetAABB(worldPos.x, worldPos.y, worldPos.z);
        }
        
        public virtual AABB GetAABB(int x, int y, int z)
        {
            return LuckyPool.GetAABBFromPool(aabb.minX + x, aabb.minY + y, aabb.minZ + z,
                                             aabb.maxX + x, aabb.maxY + y, aabb.maxZ + z);
        }

        public virtual void OnBeforePlace(ref IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
        }
        
        public virtual void OnAfterPlace(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            level.PlaySoundAt(soundDescriptor.placeSoundGroup.GetPath(), worldPos.x, worldPos.y, worldPos.z);
        }

        public virtual bool OnInteract(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            return true;
        }
        
        public virtual void OnUpdate(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            
        }

        public virtual bool OnHurt(IntVector3 worldPos, Level level, Chunk chunk, Player player)
        {
            return true;
        }

        public virtual BlockMesh OnRender(IntVector3 worldPos)
        {
            return null;
        }
    }

    public class StaticBlockBehavior : BlockBehavior
    {
        public StaticBlockBehavior(BlockSoundDescriptor blockSoundDescriptor = null) : base(blockSoundDescriptor)
        {
            
        }

        public override BlockBehavior CreateInstance()
        {
            return this;
        }
    }
}