using System.Collections.Generic;
using System.IO;
using Block2nd.Persistence.KNBT;

namespace Block2nd.GameSave
{
    public class LevelSaveManager
    {
        public LevelSavePreview GetLevelSavePreview(string saveFolderPath)
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

            preview.name = levelKnbt.GetString("Name");
            preview.folderName = Path.GetFileName(saveFolderPath);

            return preview;
        }
        
        public LevelSavePreview[] GetAllLevelSavePreviews(string levelName)
        {
            List<LevelSavePreview> previews = new List<LevelSavePreview>();
            
            var saveRoot = GameRootDirectory.GetInstance().saveRoot;

            if (!Directory.Exists(saveRoot))
            {
                Directory.CreateDirectory(saveRoot);
                return new LevelSavePreview[] {};
            }
            
            foreach (var dir in Directory.EnumerateDirectories(saveRoot))
            {
                var preview = GetLevelSavePreview(dir);
                if (preview is null) 
                    continue;
                previews.Add(preview);
            }

            return previews.ToArray();
        } 
    }
}