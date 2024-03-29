using UnityEngine;

namespace Block2nd.GUI.Hierarchical.Buttons
{
    public class QuitGameButton : MonoBehaviour
    {
        public void OnClick()
        {
            Application.Quit();
        }
    }
}