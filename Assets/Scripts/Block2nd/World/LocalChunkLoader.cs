using System.IO;
using System.IO.Compression;
using Block2nd.Persistence.KNBT;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Block2nd.World
{
    public class LocalChunkLoader : IChunkLoader
    {
        public Chunk TryLoadChunk(Level level, int chunkX, int chunkZ)
        {
            if (level is null)
                return null;

            var path = level.levelSaveHandler.GetChunkFilePath(chunkX, chunkZ);

            if (!File.Exists(path))
            {
                return null;
            }
            
            var gzipStream = new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress);
            var reader = new BinaryReader(gzipStream);

            var knbt = new KNBTTagCompound("Chunk");
            knbt.Read(reader);
            
            reader.Dispose();
            gzipStream.Dispose();

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
            
            chunk.populateState = knbt.GetInt("PopulateState");
            chunk.dirty = false;
            chunk.modified = false;
            chunk.saved = true;
            
            return chunk;
        }

        public void SaveChunk(Level level, Chunk chunk)
        {
            var knbt = chunk.GetChunkKNBTData();

            var path = level.levelSaveHandler.GetChunkFilePath(
                chunk.worldBasePosition.x >> 4, chunk.worldBasePosition.z >> 4);
            
            var gzipStream = new GZipStream(new FileStream(path, FileMode.OpenOrCreate), CompressionLevel.Fastest);
            var writer = new BinaryWriter(gzipStream);
            
            knbt.Write(writer);
            
            writer.Dispose();
            gzipStream.Dispose();

            chunk.saved = true;
        }
    }
}