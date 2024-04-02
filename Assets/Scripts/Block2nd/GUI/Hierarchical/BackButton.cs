using UnityEngine;

namespace Block2nd.GUI.Hierarchical
{
    public class BackButton : MonoBehaviour
    {
        private HierarchicalMenu hierarchicalMenu;

        private void Awake()
        {
            hierarchicalMenu = GameObject.FindGameObjectWithTag("Menu").GetComponent<HierarchicalMenu>();
        }

        public void ExitPage()
        {
            hierarchicalMenu.ExitPage();
        }
    }
}