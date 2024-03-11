using System;
using Block2nd.Database;
using UnityEngine;

namespace Block2nd.World
{
    public class TerrainGenerator
    {
        public int waterLevel = 6;
        
        public int seed = 10727;
        public int minHeight = 5;
        public float noiseFreq = 0.025f;

        public WorldSettings worldSettings;
        
        private System.Random rand = new System.Random();

        private Vector2 sampleOffset;

        public TerrainGenerator(WorldSettings worldSettings)
        {
            Noise2d.Reseed();
            seed += rand.Next(1000000, 9999999);
            this.worldSettings = worldSettings;

            sampleOffset = new Vector2(rand.Next(100000, 999999) / 1000000f, 
                                        rand.Next(100000, 999999) / 1000000f);
        }

        public void ResetOffset()
        {
            sampleOffset = new Vector2(rand.Next(100000, 999999) / 1000000f, 
                rand.Next(100000, 999999) / 1000000f);
        }

        public int GetHeightPerlin(float x, float z)
        {
            x += sampleOffset.x;
            z += sampleOffset.y;
            
            var plain = Mathf.PerlinNoise(x * 10, z * 5) * 20;
            var plain2 = Mathf.Clamp(Mathf.PerlinNoise(x * 30, z * 30) - 0.1f, 0, 1) * 5;
            var mountain = Mathf.Clamp(Mathf.PerlinNoise(10 * x, 10 * z) - 0.5f, 0, 1) * 10;
            var mountain2 = Mathf.Clamp(Mathf.PerlinNoise(5 * x, 15 * z) - 0.7f, 0, 1) * 20;
            var mountain3 = Mathf.Clamp(Mathf.PerlinNoise(20 * x, 17 * z) - 0.6f, 0, 1) * 25;
            
            var erode1 = -Mathf.Clamp(Mathf.PerlinNoise(48 * x, 32 * z) - 0.8f, 0, 1) * 10;
            var erode2 = -Mathf.Clamp(Mathf.PerlinNoise(32 * x, 40 * z) - 0.6f, 0, 1) * 10;
            
            var riverDown = Mathf.Clamp01(Mathf.PerlinNoise(8 * x, 8 * z)) * (20 + waterLevel / 2);

            var h = (int) (waterLevel + plain + plain2 + mountain + mountain2 + mountain3 + 
                            erode1 + erode2 - riverDown);

            return h > 0 ? h : 1;
        }

        public int GetErodeDepthPerlin(float x, float z)
        {
            x += sampleOffset.x;
            z += sampleOffset.y;

            var erode = Mathf.Clamp01(Mathf.PerlinNoise(40 * x, 40 * z) - 0.5f);

            return (int) (erode > 0.2 ? 1 : 0) * 5;
        } 
        
        public int GetRiverInfoPerlin(float x, float z)
        {
            x += sampleOffset.x;
            z += sampleOffset.y;

            var erode = Mathf.Clamp01(Mathf.PerlinNoise(8 * x, 8 * z));
            var erode2 = Mathf.Clamp01(Mathf.PerlinNoise(20 * x, 20 * z));

            return (int)((erode < 0.30f ? erode + erode2 : 0) * 10);
        } 

        public int GetHeightHonkai(float x, float z)
        {
            x += rand.Next(100000, 999999) / 1000000f;
            z += rand.Next(100000, 999999) / 1000000f;
            var plain = Mathf.PerlinNoise(x * 10, z * 5) * 20;
            x += rand.Next(100000, 999999) / 1000000f;
            z += rand.Next(100000, 999999) / 1000000f;
            var plain2 = Mathf.Clamp(Mathf.PerlinNoise(x * 5, z * 5) - 0.1f, 0, 1) * 5;
            x += rand.Next(100000, 999999) / 1000000f;
            z += rand.Next(100000, 999999) / 1000000f;
            var mountain = Mathf.Clamp(Mathf.PerlinNoise(10 * x, 10 * z) - 0.5f, 0, 1) * 50;
            x += rand.Next(100000, 999999) / 1000000f;
            z += rand.Next(100000, 999999) / 1000000f;
            var mountain2 = Mathf.Clamp(Mathf.PerlinNoise(2 * x, 8 * z) - 0.7f, 0, 1) * 80;

            return (int) (3 + plain + plain2 + mountain + mountain2);
        }

        public int _GetHeightPerlin(float x, float z)
        {
            var plain = Mathf.Clamp(Noise2d.PerlinNoise(x * 3, z * 3) + 0.2f, 0, 2) * 10;
            
            var plain2 = Mathf.Clamp(Noise2d.PerlinNoise(x * 3, z * 3) - 0.1f, 0, 1) * 20;

            var mountain = Mathf.Clamp(Noise2d.PerlinNoise(6 * x, 6 * z) - 0.3f, 0, 1) * 80;
            
            var mountain2 = Mathf.Clamp(Noise2d.PerlinNoise(20 * x, 20 * z) - 0.2f, 0, 1) * 100;

            return (int) (3 + plain + plain2 + mountain + mountain2);
        }

        public int GetHeightRandom(float x, float z)
        {
            return rand.Next(2, 6);
        }

        public int GetHeightFlat(float x, float z)
        {
            return 5;
        }

        public virtual int GetHeight(float x, float z)
        {
            return GetHeightPerlin(x, z);
        }
    }

    public class HonkaiTerrainGenerator : TerrainGenerator
    {
        public HonkaiTerrainGenerator(WorldSettings worldSettings) : base(worldSettings)
        { }

        public override int GetHeight(float x, float z)
        {
            return GetHeightHonkai(x, z);
        }
    }
    
    public class FlatTerrainGenerator : TerrainGenerator
    {
        public FlatTerrainGenerator(WorldSettings worldSettings) : base(worldSettings)
        { }

        public override int GetHeight(float x, float z)
        {
            return GetHeightFlat(x, z);
        }
    }
}