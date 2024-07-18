using System;
using Block2nd.Phys;
using UnityEngine;
using UnityEngine.UI;

namespace TestPhys
{
    [ExecuteInEditMode]
    public class Tester : MonoBehaviour
    {
        public Text aabbText;
        public Text camText;
        public Text aabbHitText;
        public Text unityHitText;

        private AABB aabb = new AABB(-1.5f, -1, -1.5f, -.5f, 1, -.5f);
        
        private void Start()
        {
            Debug.Log(aabb.Center);
        }

        private void Update()
        {
            aabbText.text = "AABB: " + aabb.ToString() + " | Center: " + aabb.Center;
            camText.text = "Cam Center: " + Camera.main.transform.position + " | Forward: " +
                           Camera.main.transform.forward;
            
            transform.position = aabb.Center;
            
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 15);

            // builtin unity hit
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward * 15, out hit))
            {
                Debug.DrawRay(hit.point, hit.normal, Color.red);
                unityHitText.text = "Unity Hit: " + hit.point + " | Normal: " + hit.normal;
            }
            else
            {
                unityHitText.text = "[None]";
            }

            // b2nd hit
            RayHit hit2 = aabb.Raycast(Camera.main.transform.position, 
                Camera.main.transform.position + Camera.main.transform.forward * 15);
            if (hit2 != null)
            {
                Debug.DrawRay(hit2.point, RayHit.Normals[hit2.normalDirection], Color.green);
                aabbHitText.text = "B2nd Hit: " + hit2.point + " | Normal: " + 
                                   RayHit.Normals[hit2.normalDirection] + " (" + hit2.normalDirection+ ")";
            }
            else
            {
                aabbHitText.text = "[None]";
            }
        }
    }
}