using System;
using System.IO;
using Block2nd.GameSave;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace TitlePage
{
    public class SplashText : MonoBehaviour
    {
        [Range(25, 100)] public float twinklingSpeed = 1;
        [Range(0, 1)] public float minSize = 0.45f;

        private Random random = new Random();
        
        private Text text;
        private RectTransform rectTransform;

        private void Awake()
        {
            text = GetComponent<Text>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            TrySetTextFromSplashesFile();
        }

        private void Update()
        {
            rectTransform.localScale = Vector3.one * 
                                       (2 - (Mathf.Abs(minSize * Mathf.Sin(Time.time * twinklingSpeed / 10f)) + 1));
        }

        private void TrySetTextFromSplashesFile()
        {
            var fileName = Path.Combine(GameRootDirectory.GetInstance().gameDataRoot, "splashes.txt");

            if (!File.Exists(fileName))
            {
                Debug.Log("splash file not exist.");
                return;
            }

            string[] lines = File.ReadAllLines(fileName);

            var winner = lines[random.Next(0, lines.Length - 1)];
            text.text = winner;
            
            Debug.Log("loaded splash: " + winner);
        }
    }
}