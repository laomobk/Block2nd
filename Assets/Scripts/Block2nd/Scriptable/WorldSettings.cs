using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Database
{
    [CreateAssetMenu(fileName = "New World Settings", menuName = "Block2nd/Settings/World")]
    [Serializable]
    public class WorldSettings : ScriptableObject
    {
        public int chunkWidth = 16;
        public int chunkHeight = 128;

        public int levelWidth = 256;
    }
}