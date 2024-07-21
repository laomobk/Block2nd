using UnityEngine;

namespace Block2nd.Utils
{
    public static class GameObjectUtils
    {
        public static void SetLayerToAllChildrenAndSelf(Transform root, int layerMask)
        {
            root.gameObject.layer = layerMask;

            foreach (Transform transform in root.GetComponentsInChildren<Transform>())
            {
                transform.gameObject.layer = layerMask;
            }
        }
    }
}