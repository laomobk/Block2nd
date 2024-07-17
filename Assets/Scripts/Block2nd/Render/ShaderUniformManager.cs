using System;
using UnityEngine;

namespace Block2nd.Render
{
    public class ShaderUniformManager : MonoBehaviour
    {
        [SerializeField] private Material skyBoxMaterial;
        
        public Color skyLightColor;
        public Color blockLightColor;
        public Color skyHorizonColor;
        public Color heavenColor;

        private int updateTick = 10;
        
        private static readonly int SkyLightColor = Shader.PropertyToID("_SkyLightColor");
        private static readonly int BlockLightColor = Shader.PropertyToID("_BlockLightColor");
        private static readonly int HorizonColor = Shader.PropertyToID("_HorizonColor");
        private static readonly int TopColor = Shader.PropertyToID("_TopColor");
        private static readonly int SkyLightLuminance = Shader.PropertyToID("_SkyLightLuminance");
        private static readonly int SkyHorizonColor = Shader.PropertyToID("_SkyHorizonColor");

        public static ShaderUniformManager Instance { get; private set; }

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
                Shader.SetGlobalColor(SkyLightColor, skyLightColor);
                Shader.SetGlobalColor(BlockLightColor, blockLightColor);
                Shader.SetGlobalColor(SkyHorizonColor, skyHorizonColor * skyLightColor);
                Shader.SetGlobalFloat(SkyLightLuminance, skyLightColor.grayscale);
                
                skyBoxMaterial.SetColor(HorizonColor, skyHorizonColor * skyLightColor);
                skyBoxMaterial.SetColor(TopColor, heavenColor * skyLightColor);
            }
        }
    }
}