using Block2nd.Client;
using UnityEngine;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class TeleportPlayerButton : MonoBehaviour
    {
        public void TeleportPlayer()
        {
            GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>().Player.RandomTeleportPlayer();
        }
    }
}