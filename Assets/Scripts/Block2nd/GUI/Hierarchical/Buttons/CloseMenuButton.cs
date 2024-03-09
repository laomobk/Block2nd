using Block2nd.Client;
using UnityEngine;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class CloseMenuButton : MonoBehaviour
    {
        public void CloseMenu()
        {
            GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>().CloseMenu();
        }
    }
}