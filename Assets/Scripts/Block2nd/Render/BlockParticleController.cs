using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Render
{
    public class BlockParticleController : MonoBehaviour
    {   
        public Texture particleTexture;
        public Shader particleShader;

        private void Start()
        {
            GetComponent<ParticleSystemRenderer>().material.shader = particleShader;
            GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", particleTexture);
        }

        private void OnDestroy()
        {
            DestroyImmediate(GetComponent<ParticleSystemRenderer>().material, true);
        }

        public Material GetMaterial()
        {
            return GetComponent<ParticleSystemRenderer>().material;
        }
    }
}