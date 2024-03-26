using Block2nd.Persistence.KNBT;
using UnityEngine;

namespace Block2nd.World
{
    public class LocalChunkLoader : IChunkLoader
    {
        private bool brandNew;

        public LocalChunkLoader(bool brandNew = true)
        {
            this.brandNew = brandNew;
        }

        public Chunk TryLoadChunk(Level level, int chunkX, int chunkZ)
        {
            if (brandNew)
            {
                return null;
            }

            if (level is null)
                return null;

            var reader = level.levelSaveHandler.GetChunkFileReader(chunkX, chunkZ);

            if (reader == null)
            {
                return null;
            }

            var knbt = new KNBTTagCompound("Chunk");
            knbt.Read(reader);

            var blocks = knbt.GetChunkBlockDataTensor("Blocks");
            if (blocks == null)
            {
                return null;
            }

            var height = knbt.GetInt("Height", 128);
            
            Chunk chunk = new Chunk(level, chunkX, chunkZ);
            chunk.chunkBlocks = blocks;
            chunk.aabb = new Bounds(
                new Vector3(8, height / 2f, 8),
                new Vector3(16, height, 16)
            );
            chunk.BakeHeightMap();

            chunk.dirty = false;
            chunk.fromLocal = true;
            
            return chunk;
        }

        public void SaveChunk(Level level, Chunk chunk)
        {
            var knbt = chunk.GetChunkKNBTData();

            var writer = level.levelSaveHandler.GetChunkFileWriter(
                chunk.worldBasePosition.x >> 4, chunk.worldBasePosition.z >> 4);
            
            knbt.Write(writer);
            writer.Dispose();
        }
    }
}