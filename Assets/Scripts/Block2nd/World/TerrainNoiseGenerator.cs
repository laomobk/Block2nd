﻿using System;
using Block2nd.Database;
using Block2nd.Scriptable;
using UnityEngine;

namespace Block2nd.World
{
    public class TerrainNoiseGenerator
    {
        public int waterLevel = 25;
        public int baseHeight = 25;
        
        public int seed = 10727;
        public int minHeight = 5;
        public float noiseFreq = 0.025f;

        public WorldSettings worldSettings;
        
        private System.Random rand;
        private Perlin perlin;

        private Vector2 sampleOffset;

        public TerrainNoiseGenerator(WorldSettings worldSettings)
        {
            Debug.Log("NoiseGen Seed: " + worldSettings.seed);
            rand = new System.Random(worldSettings.seed);
            
            perlin = new Perlin(worldSettings.seed);
            
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

        private float Clamp01(float x, float a, float b = 1f)
        {
            return x < a ? 0 : a + Mathf.Clamp(x - a, 0, b);
        }

        public float GetHeightPerlin(float x, float z)
        {
            x += sampleOffset.x;
            z += sampleOffset.y;
            
            var plain = Mathf.PerlinNoise(x * 10, z * 5) * 20;
            var plain2 = Mathf.Clamp(Mathf.PerlinNoise(x * 30, z * 30) - 0.1f, 0, 1) * 5;
            
            var mountainHuge = Mathf.Clamp(Mathf.PerlinNoise(1 * x, 1.5f * z) - 0.7f, 0, 1) * 120;
            var mountain = Mathf.Clamp(Mathf.PerlinNoise(3 * x, 2 * z) - 0.5f, 0, 1) * 80;
            var mountain2 = Mathf.Clamp(Mathf.PerlinNoise(5 * x, 15 * z) - 0.5f, 0, 1) * 25;
            var mountain3 = Mathf.Clamp(Mathf.PerlinNoise(20 * x, 17 * z) - 0.6f, 0, 1) * 30;

            var mountain23 = Mathf.Lerp(mountain2, mountain3, 0.5f);
            
            var erode1 = -Mathf.Clamp(Mathf.PerlinNoise(48 * x, 32 * z) - 0.8f, 0, 1) * 10;
            var erode2 = -Mathf.Clamp(Mathf.PerlinNoise(32 * x, 40 * z) - 0.6f, 0, 1) * 10;
            var erode3 = -Mathf.Clamp(Mathf.PerlinNoise(50 * x, 45 * z) - 0.1f, 0, 1) * 8;
            
            var riverDown = Mathf.Clamp01(Mathf.Sqrt(Mathf.PerlinNoise(10 * x, 10 * z))) * (15 + waterLevel / 1.2f);
            // var seaDown = Mathf.Clamp01(Mathf.Sqrt(Mathf.PerlinNoise(1 * x, 1 * z))) * (12 + waterLevel / 1.2f);

            var h = (baseHeight + 
                     Mathf.Lerp(
                         waterLevel + plain + plain2 + mountainHuge + mountain + mountain23 + 
                         erode1 + erode2 + erode3 - riverDown / 3f,
                         -riverDown,
                         0.45f));

            return h > 0 ? h : 1;
        }

        private float Remap01(float x, float offset = 0f)
        {
            return Mathf.Clamp01(x * .5f + .5f - offset);
        }

        private float ClampNg0(float x, float offset = 0f)
        {
            return x > 0 ? 0 : x - offset;
        }

        private float Range(float x, float a, float b, float otherwise = 0f)
        {
            return x > a && x < b ? x : otherwise;
        }

        public float GetHeightPerlinNew(float x, float y)
        {
            var baseNoise = Remap01(perlin.Noise(x * 0.5f, y * 0.5f)) * baseHeight;
            
            var plain = 16 + Remap01(perlin.Noise(x * 2, y * 2)) * 3;
            var hill = Remap01(perlin.Fbm(x * 10, y * 10, 5), 0.7f) * 2;

            var mountainVal = Mathf.Pow(Remap01(perlin.Fbm(x * 3, y * 3, 5)), 8f);
            var mountain = mountainVal * 350f;
            
            var erode = mountainVal * hill * Remap01(perlin.Fbm(30 * x, 30 * y, 2), 0.3f) * 3f;
            var riverDown = -Mathf.Pow(Remap01(perlin.Noise(x * 10, y * 5), 0.3f), 2) * 60f;
            
            var riverDown2 = -Mathf.Pow(Remap01(perlin.Noise(x * 6, y * 6), 0.5f), 2) * 90f;

            var height = baseNoise + plain + hill + mountain + riverDown + riverDown2 - erode;

            return height > 0 ? height : 1;
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

        public float GetNoodleWith3DPerlinNoise(float x, float y, float z)
        {
            // var baseCheese = Mathf.Pow(Remap01(perlin.Fbm(x * 5, y * 5, z * 5, 2)), 3);
            var baseNoodle = Mathf.Pow(Remap01(perlin.Fbm(x * 5, y * 2, z * 2, 4)), 5);

            // var cheese = Range(baseCheese, 0.2f, 0.4f, 0);
            var noodle = Range(baseNoodle, 0.1f, 0.12f);

            var summary = noodle;
            
            return summary > 0 ? 1 : 0;
        }
        
        public float GetCheeseWith3DPerlinNoise(float x, float y, float z)
        {
            // var baseCheese = Mathf.Pow(Remap01(perlin.Fbm(x * 5, y * 5, z * 5, 2)), 3);
            var baseCheese = Mathf.Pow(Remap01(perlin.Fbm(x * 5, y * 2, z * 2, 4)), 5);

            return baseCheese;
        }

        public float GetNoodle3D(float x, float y, float z)
        {
            return GetNoodleWith3DPerlinNoise(x, y, z);
        }

        public float GetCheese3D(float x, float y, float z)
        {
            return GetCheeseWith3DPerlinNoise(x, y, z);
        }

        public int GetHeightRandom(float x, float z)
        {
            return rand.Next(2, 6);
        }

        public int GetHeightFlat(float x, float z)
        {
            return baseHeight;
        }

        public virtual float GetHeight(float x, float z)
        {
            return GetHeightPerlinNew(x, z);
        }
    }

    public class HonkaiTerrainNoiseGenerator : TerrainNoiseGenerator
    {
        public HonkaiTerrainNoiseGenerator(WorldSettings worldSettings) : base(worldSettings)
        { }

        public override float GetHeight(float x, float z)
        {
            return GetHeightHonkai(x, z);
        }
    }
    
    public class FlatTerrainNoiseGenerator : TerrainNoiseGenerator
    {
        public FlatTerrainNoiseGenerator(WorldSettings worldSettings) : base(worldSettings)
        { }

        public override float GetHeight(float x, float z)
        {
            return GetHeightFlat(x, z);
        }
    }

    public class TestTerrainNoiseGenerator : FlatTerrainNoiseGenerator
    {
        public TestTerrainNoiseGenerator(WorldSettings worldSettings) : base(worldSettings)
        {
        }

        public override float GetHeight(float x, float z)
        {
            return GetHeightFlat(x, z);
        }
    }
}