using System;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI.Hierarchical
{
    public class HierarchicalMenu : MonoBehaviour
    {
        public GameObject firstMenuPagePrefab;

        public bool openAtStart = false;
        
        private GameObject currentPage;
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
            image.enabled = false;
        }

        private void Start()
        {
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
        }
        
        public void EnterPage(GameObject pagePrefab)
        {
            DestroyImmediate(currentPage);

            currentPage = Instantiate(pagePrefab, transform);
        }

        public void SetBackgroundState(bool state)
        {
            image.enabled = state;
        }
    }
}