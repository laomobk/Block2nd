using System;
using Block2nd.Client;
using Block2nd.Database;
using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.Render
{
    public class CameraInWaterPost : MonoBehaviour
    {
        public Material postMaterial;

        private int waterCode;
        private Player player;
        private GameClient client;

        
        private void Awake()
        {
            waterCode = BlockMetaDatabase.GetBlockCodeById("b2nd:block/water");
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            client = GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>();
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (client.CurrentLevel != null)
            {
                postMaterial.SetInt("_InWater", client.CurrentLevel.GetBlock(
                    player.playerCamera.transform.position).blockCode == waterCode
                    ? 1
                    : 0);
                Graphics.Blit(src, dest, postMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }
    }
}