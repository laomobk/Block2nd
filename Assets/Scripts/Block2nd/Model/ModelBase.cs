using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Model
{
    public abstract class ModelBase
    {
        protected List<ModelRenderer> renderers = new List<ModelRenderer>();
        protected string textureId;

        public List<ModelRenderer> Renderers => renderers;
        
        protected ModelBase(string textureId)
        {
            this.textureId = textureId;
        }

        protected BoxModelRenderer AddBox(
            Vector3 offset, Vector3 localPos, float depth, float width, float height,
            float texX, float texY, float uvDepth, float uvWidth, float uvHeight)
        {
            BoxModelRenderer renderer;
            
            renderers.Add(renderer = new BoxModelRenderer(
                depth, width, height, texX, texY,uvDepth, uvWidth, uvHeight, offset, localPos));

            return renderer;
        }
    }
}