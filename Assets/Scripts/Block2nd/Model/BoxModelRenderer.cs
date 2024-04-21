using UnityEngine;

namespace Block2nd.Model
{
    public class BoxModelPart: ModelPart
    {
        private Vector3 offset;
        private float height, width;

        public BoxModelPart(float height, float width, Vector3 offset)
        {
            this.offset = offset;
            this.height = height;
            this.width = width;
        }

        public override Mesh GetMesh()
        {
             var mesh = new Mesh();

             return mesh;
        }
    }
}