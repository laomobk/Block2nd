using System;
using System.Collections;
using Block2nd.Database;
using Block2nd.Scriptable;
using Block2nd.World;
using UnityEngine;
using UnityEngine.UI;

namespace TestTerrain
{
    public class Generate3DTerrainHandler : MonoBehaviour
    {
        public RawImage outputUIImage;
        public WorldSettings worldSettings;
        public Text progressText;
        public Slider slider;

        private TerrainNoiseGenerator generator;

        private Texture2D terrainTexture;

        private void Start()
        {
            var levelWidth = 128;
            slider.maxValue = 16;
            worldSettings.levelWidth = levelWidth;
            terrainTexture = new Texture2D(levelWidth, levelWidth);
            Generate();
        }

        private IEnumerator GenerateCoroutine(int z)
        {
            outputUIImage.texture = terrainTexture;

            worldSettings.seed = (int)DateTime.Now.Ticks;

            var levelWidth = worldSettings.levelWidth;

            bool water = true;

            for (int x = 0; x < levelWidth; ++x)
            {
                for (int y = 0; y < levelWidth; ++y)
                {
                    var rawHeight = generator.GetCheese3D(
                        (float) x / levelWidth, (float) y / levelWidth, (float)z / levelWidth);
                    
                    terrainTexture.SetPixel(x, y, new Color(rawHeight, rawHeight, rawHeight));
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

        public void OnSliderValueChange()
        {
            StartCoroutine(GenerateCoroutine((int)(slider.value * slider.maxValue)));
        }

        public void Generate()
        {
            generator = new TerrainNoiseGenerator(worldSettings);
            StartCoroutine(GenerateCoroutine((int)(slider.value * slider.maxValue)));
        }
    }
}