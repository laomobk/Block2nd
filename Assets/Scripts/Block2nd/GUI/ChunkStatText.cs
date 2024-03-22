using System;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class ChunkStatText : MonoBehaviour
    {
        private Text text;

        private int chunksInCache;
        private int chunksInRender;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        private void UpdateText()
        {
            text.text = "C: " + chunksInRender + "/" + chunksInCache;
        }

        public void SetChunksInCache(int n)
        {
            chunksInCache = n;
            UpdateText();
        }

        public void SetChunksInRender(int n)
        {
            chunksInRender = n;
            UpdateText();
        }
    }
}