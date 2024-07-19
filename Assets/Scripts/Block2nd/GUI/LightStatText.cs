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
            var (ix, iy, iz) = GameClient.Instance.player.IntPosition;
            var level = GameClient.Instance.CurrentLevel;
            if (level == null)
                return;
            
            var skyLight = level.GetSkyLight(
                ix, iy, iz, true);
            var blockLight = level.GetBlockLight(
                ix, iy, iz, true);

            var levelTime = level.levelTime;

            text.text = $"L: {Mathf.Max(skyLight, blockLight)} ({skyLight} sky, {blockLight} block) | " +
                        $"{ShaderUniformManager.Instance.skyLightColor.grayscale:0.00} LM | LT: {levelTime} ";
        }
    }
}