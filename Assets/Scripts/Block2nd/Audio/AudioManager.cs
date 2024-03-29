using System;
using UnityEngine;

namespace Block2nd.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource BGMAudioSource;

        public float PlayAudio(string resPath)
        {
            try
            {
                var bgm = Resources.Load<AudioClip>(resPath);
                if (bgm == null)
                {
                    Debug.LogWarning("[Audio Manager] file not found: " + resPath);
                    return 0;
                }
                BGMAudioSource.clip = bgm;
                BGMAudioSource.Play();
                
                Debug.Log("[Audio Manager] play bgm: " + resPath);

                return bgm.length;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Audio Manager: " + e);
            }

            return 0;
        }
    }
}