using System;
using System.Collections.Generic;
using System.IO;
using Block2nd.Persistence.KNBT;
using UnityEngine;

namespace Block2nd.GameSave
{
    public static class LevelSaveManager
    {
        public static LevelSavePreview GetLevelSavePreview(string saveFolderPath)
        {
            var levelDatPath = Path.Combine(saveFolderPath, "Level.dat");
            if (!File.Exists(levelDatPath))
            {
                return null;
            }
            
            var info = new FileInfo(levelDatPath);
            
            var reader = new BinaryReader(new FileStream(levelDatPath, FileMode.Open, FileAccess.Read));
            
            var levelKnbt = new KNBTTagCompound("Level");
            levelKnbt.Read(reader);
            
            var preview = new LevelSavePreview();

            var levelName = levelKnbt.GetString("Name");

            if (levelName.Length == 0)
            {
                levelName = "<no name>";
            }

            preview.name = levelName;
            preview.folderName = Path.GetFileName(saveFolderPath);
            preview.terrainType = levelKnbt.GetInt("Type");
            preview.lastWriteTime = info.LastWriteTime;

            return preview;
        }
        
        public static List<LevelSavePreview> GetAllLevelSavePreviews(bool sort = true)
        {
            List<LevelSavePreview> previews = new List<LevelSavePreview>();
            
            var saveRoot = GameRootDirectory.GetInstance().saveRoot;

            if (!Directory.Exists(saveRoot))
            {
                Directory.CreateDirectory(saveRoot);
                return null;
            }
            
            foreach (var dir in Directory.EnumerateDirectories(saveRoot))
            {
                var preview = GetLevelSavePreview(dir);
                if (preview is null) 
                    continue;
                previews.Add(preview);
            }

            if (sort)
            {
                previews.Sort((preview1, preview2) => 
                    -preview1.lastWriteTime.Ticks.CompareTo(preview2.lastWriteTime.Ticks));
            }

            return previews;
        }

        public static string GetLevelFolderName(string name)
        {
            bool again = true;

            while (again)
            {
                again = false;
                foreach (var dir in Directory.EnumerateDirectories(GameRootDirectory.GetInstance().saveRoot))
                {
                    var folderName = Path.GetFileName(dir);
                    if (folderName == name)
                    {
                        name += '-';
                        again = true;
                        break;
                    }
                }
            }

            return name;
        }

        public static string GetLevelFolderPath(string levelFolderName)
        {
            return Path.Combine(GameRootDirectory.GetInstance().saveRoot, levelFolderName);
        }

        public static void RenameSave(LevelSavePreview preview, string newName)
        {
            var handler = new LevelSaveHandler(preview.folderName, false);
            
            var reader = handler.GetLevelDataReader();
            if (reader is null)
                return;

            var knbt = new KNBTTagCompound("Level");
            knbt.Read(reader);
            reader.Dispose();
            
            knbt.SetString("Name", newName);

            var writer = handler.GetLevelDataWriter();
            if (writer is null)
                return;
            
            knbt.Write(writer);
            writer.Dispose();
        }

        public static void DeleteSave(LevelSavePreview preview)
        {
            var path = GetLevelFolderPath(preview.folderName);
            
            Directory.Delete(path, true);
        }
    }
}