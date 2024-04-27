using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Resource
{
    public static class ResourceManager
    {
        private static readonly Dictionary<string, Object> ResourceCacheDict = new Dictionary<string, Object>();

        public static T Load<T>(string path) where T : Object
        {
            if (ResourceCacheDict.TryGetValue(path, out Object value))
            {
                return (T) value;
            }

            var res = Resources.Load<T>(path);
            if (res != null)
            {
                ResourceCacheDict[path] = res;
            }
            else
            {
                Debug.LogWarning("ResourceManager: resource not found: " + path);
            }

            return res;
        }
    }
}