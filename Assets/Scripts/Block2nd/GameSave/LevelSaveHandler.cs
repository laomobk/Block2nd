using System.Collections;
using System.Collections.Generic;
using System.IO;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.GameSave
{
    public class LevelSaveHandler
    {
        public string LevelSaveRoot { get; }
        public string ChunkSaveRoot { get; }

        public LevelSaveHandler(Level level) : this(level.levelFolderName) {}

        public LevelSaveHandler(string levelFolderName)
        {
            LevelSaveRoot = Path.Combine(GameRootDirectory.GetInstance().saveRoot, levelFolderName);
            ChunkSaveRoot = Path.Combine(LevelSaveRoot, "Dim1");

            if (!Directory.Exists(LevelSaveRoot))
            {
                Directory.CreateDirectory(LevelSaveRoot);
            }
        }

        public BinaryReader GetChunkFileReader(int chunkX, int chunkZ)
        {
            var regionX = chunkX >> 4;
            var regionZ = chunkZ >> 4;

            var chunkFileRoot = Path.Combine(ChunkSaveRoot, regionX.ToString(), regionZ.ToString());
            var chunkFileName = "c." + chunkX + "," + chunkZ + ".chk";

            var fullPath = Path.Combine(chunkFileRoot, chunkFileName);

            if (!File.Exists(fullPath))
            {
                return null;
            }
            
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);    
            
            return new BinaryReader(stream);
        }
        
        public BinaryWriter GetChunkFileWriter(int chunkX, int chunkZ)
        {
            var regionX = chunkX >> 4;
            var regionZ = chunkZ >> 4;

            var chunkFileRoot = Path.Combine(ChunkSaveRoot, regionX.ToString(), regionZ.ToString());
            var chunkFileName = "c." + chunkX + "," + chunkZ + ".chk";

            var fullPath = Path.Combine(chunkFileRoot, chunkFileName);

            if (!Directory.Exists(chunkFileRoot))
            {
                Directory.CreateDirectory(chunkFileRoot);
            }
            
            var stream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write);    
            
            return new BinaryWriter(stream);
        }

        public BinaryWriter GetPlayerDataWriter()
        {
            var playerDataFileName = Path.Combine(LevelSaveRoot, "Player.dat");
            
            var stream = new FileStream(playerDataFileName, FileMode.OpenOrCreate, FileAccess.Write);    
            
            return new BinaryWriter(stream);
        }
        
        public BinaryReader GetPlayerDataReader()
        {
            var playerDataFileName = Path.Combine(LevelSaveRoot, "Player.dat");
            
            var stream = new FileStream(playerDataFileName, FileMode.Open, FileAccess.Read);    
            
            return new BinaryReader(stream);
        }
        
        public BinaryWriter GetLevelDataWriter()
        {
            var levelDataFileName = Path.Combine(LevelSaveRoot, "Level.dat");
            
            var stream = new FileStream(levelDataFileName, FileMode.OpenOrCreate, FileAccess.Write);    
            
            return new BinaryWriter(stream);
        }
        
        public BinaryReader GetLevelDataReader()
        {
            var levelDataFileName = Path.Combine(LevelSaveRoot, "Level.dat");
            
            var stream = new FileStream(levelDataFileName, FileMode.Open, FileAccess.Read);    
            
            return new BinaryReader(stream);
        }
    }   
}
