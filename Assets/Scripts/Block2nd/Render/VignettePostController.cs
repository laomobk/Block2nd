using System;
using Block2nd.Client;
using UnityEngine;

namespace Block2nd.Render
{
    public class VignettePostController: MonoBehaviour
    {
        private float strength = 0.8f;

        private void FixedUpdate()
        {
            var level = GameClient.Instance.CurrentLevel;
            if (level == null)
                return;

            var (x, y, z) = GameClient.Instance.Player.IntPosition;
            var skyLight = level.GetSkyLight(x, y, z);
            var factor = Mathf.Sqrt(1 - skyLight / 15f);

            var targetStrength = 0.4f * factor + 0.2f;

            strength = Mathf.Lerp(strength, targetStrength, 0.4f);
            GameClient.Instance.GuiEffectController.SetVignetteEffectStrength(strength);
        }
    }
}