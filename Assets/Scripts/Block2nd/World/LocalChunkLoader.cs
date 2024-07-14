﻿using System.IO;
using System.IO.Compression;
using Block2nd.Persistence.KNBT;
using UnityEngine;
using UnityEngine.Profiling;
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
            
            Chunk chunk = new Chunk(level, chunkX, chunkZ, level.worldSettings.chunkHeight);

            chunk.SetChunkWithKNBTData(knbt);
            
            chunk.aabb = new Bounds(
                new Vector3(8, chunk.chunkHeight / 2f, 8),
                new Vector3(16, chunk.chunkHeight, 16)
            );
            chunk.BakeHeightMap();
            
            chunk.dirty = false;
            chunk.modified = false;
            chunk.saved = true;
            
            return chunk;
        }

        public void SaveChunk(Level level, Chunk chunk)
        {
            if (!chunk.NeedToSave)
                return;

            var knbt = chunk.GetChunkKNBTData();

            var path = level.levelSaveHandler.GetChunkFilePath(
                chunk.worldBasePosition.x >> 4, chunk.worldBasePosition.z >> 4);
            
            Profiler.BeginSample("Save Chunk");
            
            var gzipStream = new GZipStream(new FileStream(path, FileMode.OpenOrCreate), CompressionLevel.Fastest);
            var writer = new BinaryWriter(gzipStream);
            
            knbt.Write(writer);
            
            writer.Dispose();
            gzipStream.Dispose();
            
            Profiler.EndSample();

            chunk.saved = true;
            chunk.modified = false;
        }
    }
}