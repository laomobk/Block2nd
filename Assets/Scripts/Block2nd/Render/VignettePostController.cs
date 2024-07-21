using System;
using Block2nd.Client;
using UnityEngine;

namespace Block2nd.Render
{
    public class VignettePostController: MonoBehaviour
    {
        [SerializeField] private Material postMaterial;

        private float strength = 0.8f;
        private static readonly int Strength = Shader.PropertyToID("_Strength");

        private void FixedUpdate()
        {
            var level = GameClient.Instance.CurrentLevel;
            if (level == null)
                return;

            var (x, y, z) = GameClient.Instance.player.IntPosition;
            var skyLight = level.GetSkyLight(x, y, z);
            var factor = Mathf.Sqrt(1 - skyLight / 15f);

            var targetStrength = 0.3f * factor + 0.3f;

            strength = Mathf.Lerp(strength, targetStrength, 0.4f);
            
            postMaterial.SetFloat(Strength, strength);
        }
    }
}