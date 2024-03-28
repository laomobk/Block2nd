using System;
using UnityEngine;

namespace TitlePage
{
    public class CameraRotateController : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(Vector3.up, Time.deltaTime * 6f, Space.World);
            transform.Rotate(Vector3.right, 0.01f * Mathf.Sin(0.2f * Time.time), Space.Self);
        }
    }
}