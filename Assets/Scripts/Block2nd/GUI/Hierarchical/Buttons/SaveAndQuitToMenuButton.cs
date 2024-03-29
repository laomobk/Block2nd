using Block2nd.Client;
using UnityEngine;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class SaveAndQuitToMenuButton : MonoBehaviour
    {
        public void OnClick()
        {
            GameObject.FindGameObjectWithTag("GameClient").GetComponent<GameClient>().SaveAndQuitToTitle();
        }
    }
}