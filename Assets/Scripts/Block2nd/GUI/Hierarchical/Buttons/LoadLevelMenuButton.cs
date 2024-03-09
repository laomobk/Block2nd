using Block2nd.Client;
using UnityEngine;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class LoadLevelMenuButton : MonoBehaviour
    {
        public void LoadWorld()
        {
            GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>().LoadWorld();
        }
    }
}