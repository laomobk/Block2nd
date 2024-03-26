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

        public void GenerateWorld(IChunkProvider chunkProvider = null)
        {
            gameClient.EnterWorld(chunkProvider);
        }

        public void GenerateWorldNormal()
        {
            GenerateWorld();
        }

        public void GenerateWorldHonkai()
        {
            GenerateWorld(new ChunkProviderGenerateOrLoad(
                new LocalChunkLoader(),
                new HonkaiChunkGenerator(gameClient.worldSettings)));
        }
        
        public void GenerateWorldFlat()
        {
            GenerateWorld(new ChunkProviderGenerateOrLoad(
                new LocalChunkLoader(),
                new FlatChunkGenerator(gameClient.worldSettings)));
        }
    }
}