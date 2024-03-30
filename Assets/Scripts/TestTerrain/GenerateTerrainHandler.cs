using System;
using System.Collections;
using Block2nd.Database;
using Block2nd.Scriptable;
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

        private Texture2D terrainTexture;

        private void Start()
        {
            var levelWidth = 512;
            worldSettings.levelWidth = levelWidth;
            terrainTexture = new Texture2D(levelWidth, levelWidth);
            Generate();
        }

        private IEnumerator GenerateCoroutine()
        {
            outputUIImage.texture = terrainTexture;

            worldSettings.seed = (int)DateTime.Now.Ticks;
            var generator = new TerrainNoiseGenerator(worldSettings);

            var levelWidth = worldSettings.levelWidth;

            bool water = true;

            for (int x = 0; x < levelWidth; ++x)
            {
                for (int z = 0; z < levelWidth; ++z)
                {
                    var rawHeight = generator.GetHeight((float) x / levelWidth, (float) z / levelWidth);
                    var height = rawHeight / 128f;

                    if (water)
                    {
                        if (rawHeight >= generator.waterLevel && rawHeight <= generator.waterLevel + 0.05f)
                        {
                            terrainTexture.SetPixel(x, z, new Color(1f, 1f, 0.0f));
                        }
                        else if (rawHeight <= generator.waterLevel)
                        {
                            terrainTexture.SetPixel(x, z, new Color(0f, 0.0f, 0.5f));
                        }
                        else
                        {
                            terrainTexture.SetPixel(x, z, new Color(height, height, height));
                        }
                    }
                    else
                    {
                        terrainTexture.SetPixel(x, z, new Color(height, height, height));
                    }
                }

                if (x % 20 == 0)
                {
                    terrainTexture.Apply();
                    progressText.text = "Generating... " + (int) (100 * (x + 1f) / levelWidth) + "%";
                    yield return null;
                }
            }

            terrainTexture.Apply();
            progressText.text = "Done !";
        }

        public void Generate()
        {
            StartCoroutine(GenerateCoroutine());
        }
    }
}