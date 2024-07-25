using Block2nd.Database;
using Block2nd.Scriptable;
using UnityEngine;

namespace Block2nd.World
{
    public class NewLevelProvider : ILevelProvider
    {
        private int terrainType;
        
        public string levelName;
        
        public NewLevelProvider(string levelName, int terrainType)
        {
            this.terrainType = terrainType;
            this.levelName = levelName;
        }
        
        public Level ProvideLevel(GameObject levelPrefab, WorldSettings worldSettings)
        {
            var levelGameObject = Object.Instantiate(levelPrefab);
            var level = levelGameObject.GetComponent<Level>();

            level.name = levelName;
            level.SetChunkProvider(new ChunkProviderGenerateOrLoad(
                new LocalChunkLoaderSingleChunk(), BuiltinChunkGeneratorFactory.GetChunkGeneratorFromId(
                    terrainType, worldSettings)));

            return null;
        }
    }
}