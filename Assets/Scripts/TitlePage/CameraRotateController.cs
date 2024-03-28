using System;
using UnityEngine;

namespace TitlePage
{
    public class CameraRotateController : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(Vector3.up, Time.deltaTime * 5, Space.World);
            transform.Rotate(Vector3.right, Mathf.Sin(Time.deltaTime), Space.Self);
        }
    }
}