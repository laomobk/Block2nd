using System.Collections.Generic;
using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.Model
{
    public abstract class ModelRenderer
    {
        public Vector3 localPos;
        protected List<ModelRenderer> childRenderers = new List<ModelRenderer>();

        public List<ModelRenderer> ChildRenderers => childRenderers;

        protected ModelRenderer(Vector3 localPos)
        {
            this.localPos = localPos;
        }

        public abstract BlockMesh GetBlockMesh();
        
        public BoxModelRenderer AddBox(
            Vector3 offset, Vector3 localPos, float depth, float width, float height,
            float texX, float texY, float uvDepth, float uvWidth, float uvHeight)
        {
            BoxModelRenderer renderer;
            
            childRenderers.Add(renderer = new BoxModelRenderer(
                depth, width, height, texX, texY,uvDepth, uvWidth, uvHeight, offset, localPos));

            return renderer;
        }
    }
}