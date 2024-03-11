using System;
using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.Render
{
    public class CameraInWaterPost : MonoBehaviour
    {
        private Player player;
        public Material postMaterial;
        
        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            postMaterial.SetInt("_InWater", player.playerController.inWater ? 1 : 0);
            Graphics.Blit(src, dest, postMaterial);
        }
    }
}