using System;
using UnityEngine;

namespace Block2nd.Render
{
    public class ShaderUniformManager : MonoBehaviour
    {
        [Header("Global Shader Uniforms")] 
        public Texture2D terrainTexture;

        [Space(10)]
        [Header("SkyBox Color Configs")]
        [SerializeField] private Material skyBoxMaterial;
        
        public Color skyLightColor;
        public Color blockLightColor;
        public Color skyHorizonColor;
        public Color heavenColor;

        private int updateTick = 0;
        
        private static readonly int SkyLightColor = Shader.PropertyToID("_SkyLightColor");
        private static readonly int BlockLightColor = Shader.PropertyToID("_BlockLightColor");
        private static readonly int HorizonColor = Shader.PropertyToID("_HorizonColor");
        private static readonly int TopColor = Shader.PropertyToID("_TopColor");
        private static readonly int SkyLightLuminance = Shader.PropertyToID("_SkyLightLuminance");
        private static readonly int SkyHorizonColor = Shader.PropertyToID("_SkyHorizonColor");
        private static readonly int TerrainTexture = Shader.PropertyToID("_TerrainTexture");

        public static ShaderUniformManager Instance { get; private set; }

        public Texture2D GetAtlasTextureById(int texId)
        {
            return terrainTexture;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(Instance);
            }

            Instance = this;
        }

        private void Update()
        {
            if (--updateTick <= 0)
            {
                updateTick = 10;
                Shader.SetGlobalColor(SkyLightColor, skyLightColor);
                Shader.SetGlobalColor(BlockLightColor, blockLightColor);
                Shader.SetGlobalColor(SkyHorizonColor, skyHorizonColor);
                Shader.SetGlobalFloat(SkyLightLuminance, skyLightColor.grayscale);
                
                skyBoxMaterial.SetColor(HorizonColor, skyHorizonColor);
                skyBoxMaterial.SetColor(TopColor, heavenColor * skyLightColor);
                
                Shader.SetGlobalTexture(TerrainTexture, terrainTexture);
            }
        }
    }
}