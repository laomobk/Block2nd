using System;
using Block2nd.Client;
using Block2nd.World;
using UnityEngine;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class WorldGenerateButton : MonoBehaviour
    {
        private GameClient gameClient;
        
        private void Awake()
        {
            gameClient = GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>();
        }

        public void GenerateWorld(int levelWidth, TerrainGenerator terrainGenerator = null)
        {
            gameClient.worldSettings.levelWidth = levelWidth;
            gameClient.GenerateWorld(terrainGenerator);
        }

        public void GenerateWorldNormal()
        {
            GenerateWorld(384);
        }

        public void GenerateWorldBig()
        {
            GenerateWorld(512);
        }

        public void GenerateWorldHuge()
        {
            GenerateWorld(768);
        }

        public void GenerateWorldHonkai()
        {
            GenerateWorld(384, new HonkaiTerrainGenerator(gameClient.worldSettings));
        }
        
        public void GenerateWorldFlat()
        {
            GenerateWorld(384, new FlatTerrainGenerator(gameClient.worldSettings));
        }
    }
}