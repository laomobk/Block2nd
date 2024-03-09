using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Block2nd.GamePlay;
using Block2nd.MathUtil;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.Client
{
    [Serializable]
    public class SerializableChunkEntry
    {
        public ChunkBlockData[,,] blocks;
        public int[,] heightMap;
        public IntVector3 basePos;
    }
    
    [Serializable]
    public class GameSave
    {
        public float[] playerPosition;
        public float playerViewHorAngel;
        public float playerViewRotAngel;
        public SerializableChunkEntry[] chunkEntries;
    }
    
    public class GameSaveManager
    {
        private string persistentDataPath;
        private string saveDirectory;
        
        private BinaryFormatter binaryFormatter;

        public GameSaveManager(string persistentDataPath)
        {
            this.persistentDataPath = persistentDataPath;
            saveDirectory = persistentDataPath + "/Saves";
            
            binaryFormatter = new BinaryFormatter();
            
            CreateSaveDir();
        }

        public void CreateSaveDir()
        {
            if (Directory.Exists(saveDirectory))
                return;
            
            Directory.CreateDirectory(saveDirectory);
            UnityEngine.Debug.Log("SaveManager: save directory created.");
        }
        
        public void SaveLevel(Player player, Level level)
        {
            UnityEngine.Debug.Log("SaveManager: save level: " + level);
            
            var gameSaveObject = new GameSave();
            
            var chunkManager = level.ChunkManager;
            SerializableChunkEntry[] serializableChunkEntries = new SerializableChunkEntry[
                                                                        chunkManager.chunkEntries.Count];
            
            UnityEngine.Debug.Log("SaveManager: processing chunks data...");
            
            int idx = 0;
            foreach (var entry in chunkManager.chunkEntries)
            {
                serializableChunkEntries[idx] = new SerializableChunkEntry
                {
                    blocks = entry.chunk.chunkBlocks,
                    heightMap = entry.chunk.heightMap,
                    basePos = entry.basePos,
                };
                
                idx++;
            }
            
            UnityEngine.Debug.Log("SaveManager: processing player data...");

            var playerPos = player.transform.position;
            
            gameSaveObject.chunkEntries = serializableChunkEntries;
            gameSaveObject.playerPosition = new []{playerPos.x, playerPos.y, playerPos.z};
            gameSaveObject.playerViewHorAngel = player.horAngle;
            gameSaveObject.playerViewRotAngel = player.rotAngle;
            
            UnityEngine.Debug.Log("SaveManager: serializing...");

            using (FileStream fs = File.Create(saveDirectory + "/" + level.levelName + ".b2ndsave"))
            {
                binaryFormatter.Serialize(fs, gameSaveObject);
            }
            
            UnityEngine.Debug.Log("SaveManager: done!");
        }

        public GameSave LoadSave(string levelName)
        {
            using (var fs = File.OpenRead(saveDirectory + "/" + levelName + ".b2ndsave"))
            {
                UnityEngine.Debug.Log("SaveManager: loading save: " + 
                                      saveDirectory + "/" + levelName + ".b2ndsave");
                var saveObject = (GameSave) binaryFormatter.Deserialize(fs);
                return saveObject;
            }
        }
    }
}