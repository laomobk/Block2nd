using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.Render;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class LightStatText : MonoBehaviour
    {
        private Text text;

        // Use this for initialization
        void Start()
        {
            this.text = GetComponent<Text>();
        }

        void Update()
        {
            var playerPos = GameClient.Instance.player.Position;
            var level = GameClient.Instance.CurrentLevel;
            if (level == null)
                return;

            var skyLight = level.GetSkyLight(
                (int) playerPos.x, (int) playerPos.y, (int) playerPos.z, true);
            var blockLight = level.GetBlockLight(
                (int) playerPos.x, (int) playerPos.y, (int) playerPos.z, true);

            text.text = $"L: {Mathf.Max(skyLight, blockLight)} ({skyLight} sky, {blockLight} block) / " +
                        $"{ShaderUniformManager.Instance.skyLightColor.grayscale:0.00} LM";
        }
    }
}