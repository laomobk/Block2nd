using System.Collections.Generic;
using System.IO;
using Block2nd.Persistence.KNBT;

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

            return preview;
        }
        
        public static List<LevelSavePreview> GetAllLevelSavePreviews()
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
    }
}