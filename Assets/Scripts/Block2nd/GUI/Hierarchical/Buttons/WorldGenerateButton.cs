﻿using System;
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

        public void GenerateWorld(int levelWidth, TerrainNoiseGenerator terrainNoiseGenerator = null)
        {
            gameClient.worldSettings.levelWidth = levelWidth;
            gameClient.EnterWorld(terrainNoiseGenerator);
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
            GenerateWorld(384, new HonkaiTerrainNoiseGenerator(gameClient.worldSettings));
        }
        
        public void GenerateWorldFlat()
        {
            GenerateWorld(384, new FlatTerrainNoiseGenerator(gameClient.worldSettings));
        }
    }
}