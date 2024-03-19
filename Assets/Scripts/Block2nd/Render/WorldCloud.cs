using System;
using UnityEngine;

namespace Block2nd.Render
{
    public class WorldCloud : MonoBehaviour
    {
        private Material material;
        private Transform playerTransform;

        private void Start()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            material = GetComponent<MeshRenderer>().material;
        }

        private void Update()
        {
            var playerPos = playerTransform.position;
            transform.position = new Vector3(playerPos.x, 64, playerPos.z);
            material.SetVector("_PlayerPos", playerPos);
        }
    }
}