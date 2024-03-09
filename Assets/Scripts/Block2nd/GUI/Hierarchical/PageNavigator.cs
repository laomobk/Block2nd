using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.GUI.Hierarchical
{
    public class PageNavigator : MonoBehaviour
    {
        private HierarchicalMenu hierarchicalMenu;
        
        public GameObject targetPagePrefab;

        private void Awake()
        {
            hierarchicalMenu = GameObject.FindGameObjectWithTag("Menu").GetComponent<HierarchicalMenu>();
        }

        public void EnterTargetPage()
        {
            hierarchicalMenu.EnterPage(targetPagePrefab);
        }
    }
}