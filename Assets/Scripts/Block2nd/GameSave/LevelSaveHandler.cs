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

        public LevelSaveHandler(string levelFolderName, bool init = true)
        {
            LevelSaveRoot = Path.Combine(GameRootDirectory.GetInstance().saveRoot, levelFolderName);
            ChunkSaveRoot = Path.Combine(LevelSaveRoot, "Dim1");
            ChunkSaveRoot = Path.Combine(LevelSaveRoot, "Dim1");

            if (init && !Directory.Exists(LevelSaveRoot))
            {
                Directory.CreateDirectory(LevelSaveRoot);
            }
        }
        
        public string GetChunkFilePath(int chunkX, int chunkZ)
        {
            var regionX = chunkX >> 4;
            var regionZ = chunkZ >> 4;

            var chunkFileRoot = Path.Combine(ChunkSaveRoot, regionX.ToString(), regionZ.ToString());
            var chunkFileName = "c." + chunkX + "," + chunkZ + ".chkz";

            var fullPath = Path.Combine(chunkFileRoot, chunkFileName);

            if (!Directory.Exists(chunkFileRoot))
            {
                Directory.CreateDirectory(chunkFileRoot);
            }

            return fullPath;
        }
        
        public string GetChunkRegionPath(int chunkX, int chunkZ)
        {
            var regionX = chunkX >> 5;
            var regionZ = chunkZ >> 5;

            var chunkFileName = "r." + regionX + "," + regionZ + ".krgn";

            var fullPath = Path.Combine(ChunkSaveRoot, chunkFileName);

            return fullPath;
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

            if (!File.Exists(playerDataFileName))
                return null;
            
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
            
            if (!File.Exists(levelDataFileName))
                return null;
            
            var stream = new FileStream(levelDataFileName, FileMode.Open, FileAccess.Read);    
            
            return new BinaryReader(stream);
        }
    }   
}
