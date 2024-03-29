using System;
using UnityEngine;

namespace TitlePage
{
    public class CameraRotateController : MonoBehaviour
    {
        private float time = 0f;
        
        private void FixedUpdate()
        {
            time += Time.fixedDeltaTime;
            
            transform.Rotate(Vector3.up, Time.fixedDeltaTime * 6f, Space.World);
            transform.Rotate(Vector3.right, 0.01f * Mathf.Sin(0.2f * time), Space.Self);
        }
    }
}