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

            var played = new List<int>();
            
            while (true)
            {
                yield return new WaitForSeconds(random.Next(1, 3));
                var song = 0;

                if (played.Count <= 5)
                {
                    int count = 0;
                    do
                    {
                        song = random.Next(0, 4);
                        count++;
                    } while (played.Contains(song) && count < 6);
                }
                else
                {
                    song = random.Next(0, 4);
                    played.Clear();
                }

                float length = 0;
                switch (song)
                {
                    case 0:
                        length = audioManager.PlayAudio("newmusic/hal1");
                        break;
                    case 1:
                        length = audioManager.PlayAudio("newmusic/piano2");
                        break;
                    case 2:
                        length = audioManager.PlayAudio("music/calm1");
                        break;
                    case 3:
                        length = audioManager.PlayAudio("newmusic/hal1");
                        break;
                    case 4:
                        length = audioManager.PlayAudio("music/calm2");
                        break;
                }

                yield return new WaitForSeconds(length);
                
                played.Add(song);
            }
        }
    }
}