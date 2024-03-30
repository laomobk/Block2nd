using System;

namespace Block2nd.GameSave
{
    public class LevelSavePreview
    {
        public string name;
        public string folderName;
        public int terrainType = 0;
        public int seed = 0;
        public DateTime lastWriteTime;
        public bool newWorld;

        public override string ToString()
        {
            return $"LevelSave({name}, {folderName})";
        }
    }
}