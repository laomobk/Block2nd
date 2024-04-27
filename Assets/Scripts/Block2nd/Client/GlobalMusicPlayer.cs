using System.Collections;
using System.Collections.Generic;
using Block2nd.Audio;
using UnityEngine;
using Random = System.Random;

namespace Block2nd.Client
{
    public static class GlobalMusicPlayer
    {
        private static Random random = new Random();

        public static IEnumerator BGMPlayCoroutine()
        {
            var audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

            int lastPlayed = -1;
            
            while (true)
            {
                yield return new WaitForSeconds(random.Next(3, 9));
                int song;

                do
                {
                    song = random.Next(0, 2);
                } while (song == lastPlayed);

                float length = 0;
                switch (song)
                {
                    case 0:
                        length = audioManager.PlayAudio("music/calm1");
                        break;
                    case 1:
                        length = audioManager.PlayAudio("music/calm2");
                        break;
                    case 2:
                        length = audioManager.PlayAudio("music/calm3");
                        break;
                }

                lastPlayed = song;

                yield return new WaitForSeconds(length);
            }
        }

        public static void SetVolume(float volume)
        {
            var audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
            audioManager.SetVolume(volume);
        }
    }
}