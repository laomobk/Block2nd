using System;
using System.Collections;
using Block2nd.Database;
using Block2nd.World;
using UnityEngine;
using UnityEngine.UI;

namespace TestTerrain
{
    public class GenerateTerrainHandler : MonoBehaviour
    {
        public RawImage outputUIImage;
        public WorldSettings worldSettings;
        public Text progressText;

        private void Start()
        {
            Generate();
        }

        private IEnumerator GenerateCoroutine()
        {
            var levelWidth = worldSettings.levelWidth;
            var terrainTexture = new Texture2D(levelWidth, levelWidth);
            
            var generator = new TerrainGenerator(worldSettings);

            for (int x = 0; x < levelWidth; ++x)
            {
                for (int z = 0; z < levelWidth; ++z)
                {
                    var height = Mathf.Sqrt(
                        generator.GetHeight((float)x / levelWidth, (float)z / levelWidth) / 64f);
                    terrainTexture.SetPixel(x, z, new Color(height, height, height));
                }
                
                terrainTexture.Apply();
                outputUIImage.texture = terrainTexture;
                progressText.text = "Generating... " + (int) (100 * (x + 1f) / levelWidth) + "%";
                yield return null;
            }

            terrainTexture.Apply();
            outputUIImage.texture = terrainTexture;
            progressText.text = "Done !";
        }

        public void Generate()
        {
            StartCoroutine(GenerateCoroutine());
        }
    }
}