using System;
using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.GUI
{
    public class HandShakingBehavior : MonoBehaviour
    {
        private CharacterController characterController;

        private void Awake()
        {
            characterController = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
        }

        private void Update()
        {
            var v = characterController.velocity;
            v.y = 0;
            
            ShakeAnimation(Time.time * 1f, v.magnitude / 10f, out float x, out float y);
            
            
            Debug.Log(v.magnitude);
            transform.localPosition = new Vector3(x, y + 1f, 0);
        }

        private void ShakeAnimation(float t, float v, out float x, out float y)
        {
            x = 0.5f * Mathf.Sin(-3.14159f * t * v);
            y = -Mathf.Abs(Mathf.Cos(-t * v * 3.14159f));
        }
    }
}