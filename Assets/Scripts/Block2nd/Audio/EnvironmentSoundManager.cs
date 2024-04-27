using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Audio
{
    public class SoundGameObjectRecord
    {
        public float startTime;
        public float length;
        public AudioSource source;
    }
    
    public class EnvironmentSoundManager : MonoBehaviour
    {
        private List<SoundGameObjectRecord> records = new List<SoundGameObjectRecord>();
        
        [SerializeField] private GameObject audioSourcePrefab;

        public void PlaySoundAt(AudioClip clip, float x, float y, float z)
        {
            var record = GetFreeSoundRecord();
            record.length = clip.length;
            record.startTime = Time.time;
            
            record.source.transform.position = new Vector3(x, y, z);
            record.source.clip = clip;
            record.source.Play();
        }

        protected SoundGameObjectRecord GetFreeSoundRecord()
        {
            var curTime = Time.time;
            foreach (var record in records)
            {
                if (record.startTime + record.length < curTime - 0.2f)
                {
                    return record;
                }                
            }

            if (records.Count >= 20)
                return records[0];

            var recGameObject = Instantiate(audioSourcePrefab);
            var rec = new SoundGameObjectRecord
            {
                source = recGameObject.GetComponent<AudioSource>()
            };
            records.Add(rec);

            return rec;
        }
    }
}