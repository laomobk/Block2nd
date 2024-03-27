using System.IO;
using UnityEngine;

namespace Block2nd.GameSave
{
    public class GameRootDirectory
    {
        private static GameRootDirectory _instance;

        public readonly string saveRoot; 
        
        private GameRootDirectory()
        {
            saveRoot = Path.Combine(Application.persistentDataPath, "Saves");
        }

        public static GameRootDirectory GetInstance()
        {
            if (_instance == null)
                _instance = new GameRootDirectory();
            
            return _instance;
        }
    }
}