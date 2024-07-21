using UnityEngine;

namespace Block2nd.GUI.GameGUI
{
    public struct TransformSettings
    {
        public Vector3 localPos;
        public Vector3 localEulerAngels;
        public Vector3 localScale;

        public void Apply(Transform transform)
        {
            transform.localPosition = localPos;
            transform.localEulerAngles = localEulerAngels;
            transform.localScale = localScale;
        }
    }
}