using System;
using UnityEngine;

namespace TestBobbling
{
    public class CameraController : MonoBehaviour
    {
        private Camera camera;

        [SerializeField] private float speed;
        [SerializeField] private float dist;

        private void Awake()
        {
            camera = GetComponent<Camera>();
        }

        private void Update()
        {
            float hAxis = Input.GetAxis("Vertical");

            speed = hAxis * 2f;
            speed = Mathf.Max(0, speed - Time.deltaTime);

            dist += speed * Time.deltaTime;

            float b = Mathf.Sin(-dist * Mathf.PI) * 3f;
            float c = -Mathf.Abs(Mathf.Cos(-dist * Mathf.PI - 0.2f)) * 5;

            transform.localEulerAngles = new Vector3(c * 0.2f, 0, 0);
        }
    }
}