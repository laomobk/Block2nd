using System;
using Block2nd.Client;
using Block2nd.Database;
using Block2nd.GamePlay;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.Render
{
    public class PlayerCameraPost : MonoBehaviour
    {
        public Material inWaterPostMaterial;

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
            if (client.GameClientState != GameClientState.IN_PROGRESS && client.CurrentLevel != null)
            {
                inWaterPostMaterial.SetInt("_InWater", client.CurrentLevel.GetBlock(
                    player.playerCamera.transform.position).blockCode == waterCode
                    ? 1
                    : 0);
                Graphics.Blit(src, dest, inWaterPostMaterial);
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }
    }
}