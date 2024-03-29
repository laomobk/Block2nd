using Block2nd.Client;
using UnityEngine;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class GenerateNewWorldButton : MonoBehaviour
    {
        public void GenerateNewWorld()
        {
            GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>().CheckAndEnterWorld();
        }
    }
}