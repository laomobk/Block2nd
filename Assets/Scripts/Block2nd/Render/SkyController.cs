using System.Collections;
using System.Collections.Generic;
using Block2nd.Client;
using UnityEngine;

namespace Block2nd.Render
{
    public class SkyController : MonoBehaviour
    {
        void Start()
        {

        }

        void Update()
        {
            var playerPos = GameClient.Instance.player.Position;
            transform.position = new Vector3(playerPos.x, playerPos.y, playerPos.z);
        }
    }
}