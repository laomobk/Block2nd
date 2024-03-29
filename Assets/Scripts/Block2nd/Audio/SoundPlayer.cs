using System;
using UnityEngine;

namespace Block2nd.Audio
{
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlaySound(string resPath)
        {
            try
            {
                var bgm = Resources.Load<AudioClip>(resPath);
                if (bgm == null)
                {
                    Debug.LogWarning("[Sound Player] file not found: " + resPath);
                    return;
                }
                audioSource.clip = bgm;
                audioSource.Play();
                
                Debug.Log("[Sound Player] play bgm: " + resPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Audio Manager: " + e);
            }
        }
    }
}