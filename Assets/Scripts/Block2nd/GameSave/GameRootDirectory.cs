using System.IO;
using UnityEngine;

namespace Block2nd.GameSave
{
    public class GameRootDirectory
    {
        private static GameRootDirectory _instance;

        public readonly string saveRoot; 
        public readonly string gameDataRoot; 
        
        private GameRootDirectory()
        {
            saveRoot = Path.Combine(Application.persistentDataPath, "Saves");
            gameDataRoot = Path.Combine(Application.persistentDataPath, "Data");

            if (!Directory.Exists(saveRoot))
                Directory.CreateDirectory(saveRoot);
            
            if (!Directory.Exists(gameDataRoot))
                Directory.CreateDirectory(gameDataRoot);
        }

        public static GameRootDirectory GetInstance()
        {
            if (_instance == null)
                _instance = new GameRootDirectory();
            
            return _instance;
        }
    }
}