using System;
using UnityEngine;

namespace Block2nd.Scriptable
{
    [CreateAssetMenu(fileName = "New World Settings", menuName = "Block2nd/Settings/World")]
    [Serializable]
    public class WorldSettings : ScriptableObject
    {
        public int seed = 0;
        public int chunkHeight = 128;
        public int levelWidth = 256;
    }
}