using System;
using Block2nd.Model;
using UnityEngine;

namespace TestEntityRenderer
{
    public class Tester : MonoBehaviour
    {
        [SerializeField] private GameObject entityPrefab;

        private void Start()
        {
            var entity = Instantiate(entityPrefab);
            var manager = entity.GetComponent<EntityModelManager>();
            
            manager.Setup(new TestModel("..."));
        }
    }
}