using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical
{
    public class HierarchicalMenu : MonoBehaviour
    {
        public GameObject firstMenuPagePrefab;

        public bool openAtStart = false;

        private GameObject lastPagePrefab;
        private GameObject currentPage;
        private Image image;

        private Stack<GameObject> pagePrefabsStack = new Stack<GameObject>();

        private void Awake()
        {
            image = GetComponent<Image>();
            image.enabled = false;
        }

        private void Start()
        {
            pagePrefabsStack.Clear();
            lastPagePrefab = firstMenuPagePrefab;
            
            if (openAtStart)
                OpenFirstPage();
        }

        public void OpenFirstPage()
        {
            image.enabled = true;
            currentPage = Instantiate(firstMenuPagePrefab, transform);
        }

        public void CloseMenu()
        {
            image.enabled = false;
            DestroyImmediate(currentPage);
            pagePrefabsStack.Clear();
        }
        
        public void EnterPage(GameObject pagePrefab)
        {
            DestroyImmediate(currentPage);
            currentPage = Instantiate(pagePrefab, transform);
            pagePrefabsStack.Push(lastPagePrefab);
        }

        public void ExitPage()
        {
            if (pagePrefabsStack.Count == 0)
                CloseMenu();

            var prefab = pagePrefabsStack.Pop();
            EnterPage(prefab);
        }

        public void SetBackgroundState(bool state)
        {
            image.enabled = state;
        }
    }
}