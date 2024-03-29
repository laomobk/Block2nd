using System;
using System.Collections;
using UnityEngine;
using Random = System.Random;

namespace TitlePage
{
    public class AudioPlayer : MonoBehaviour
    {
        public AudioClip music1;
        public AudioClip music2;
        
        private AudioSource musicSource;
        private Random random = new Random();

        private void Awake()
        {
            musicSource = transform.Find("MusicSource").GetComponent<AudioSource>();
        }

        private void Start()
        {
            StartCoroutine(MusicPlayCoroutine());
        }

        private IEnumerator MusicPlayCoroutine()
        {
            var clip1 = music1;
            var clip2 = music2;

            if (random.Next(0, 2) == 1)
            {
                clip1 = music2;
                clip2 = music1;
            }
            
            if (clip1 != null)
            {
                var length = clip1.length;

                musicSource.clip = clip1;
                musicSource.Play();
                
                yield return new WaitForSeconds(length);
            }

            if (clip2 != null)
            {
                var length = clip2.length;

                musicSource.clip = clip2;
                musicSource.Play();
                
                yield return new WaitForSeconds(length);
            }
        }
    }
}