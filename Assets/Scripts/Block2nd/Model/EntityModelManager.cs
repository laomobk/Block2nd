using System;
using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Model
{
    public class EntityModelManager : MonoBehaviour
    {
        [SerializeField] private GameObject modelNodePrefab;
        [SerializeField] private Shader entityShader;
        
        private ModelBase model;
        private Material material;

        private void OnDestroy()
        {
            Destroy(material);
        }

        public void Setup(ModelBase model)
        {
            this.model = model;
            
            material = new Material(entityShader);
            
            ExpandModelToGameObjectTree();
        }

        private void ExpandModelToGameObjectTree()
        {
            var renderers = model.Renderers;
            
            var renderersStack = new Stack<ModelRenderer>();
            var parentStack = new Stack<GameObject>();

            var baseModelNode = Instantiate(modelNodePrefab, transform);
            baseModelNode.name = "_ModelBase";
            baseModelNode.transform.localPosition = Vector3.zero;

            for (int i = renderers.Count - 1; i >= 0; --i)
            {
                renderersStack.Push(renderers[i]);
                parentStack.Push(baseModelNode);
            }

            GameObject parentNode;
            ModelRenderer renderer;
            for (; renderersStack.Count > 0;)
            {
                parentNode = parentStack.Pop();
                renderer = renderersStack.Pop();
                
                baseModelNode = Instantiate(modelNodePrefab, parentNode.transform);
                baseModelNode.transform.localPosition = renderer.localPos;
                var meshFilter = baseModelNode.GetComponent<MeshFilter>();
                var meshRenderer = baseModelNode.GetComponent<MeshRenderer>();

                meshFilter.sharedMesh = renderer.GetMesh();
                meshRenderer.material = material;

                for (int i = renderer.ChildRenderers.Count - 1; i >= 0; --i)
                {
                    renderersStack.Push(renderer.ChildRenderers[i]);
                    parentStack.Push(baseModelNode);
                }
            }
        }
    }
}