using UnityEngine;

namespace Block2nd.Database.Meta
{
    public class WaterBlockShape : CubeBlockShape
    {
        public WaterBlockShape(CubeAppearance appearance) : base(appearance)
        {
        }

        public override BlockMesh GetShapeMesh(int exposedFace, int lightAttenuation)
        {
            if ((exposedFace & 16) != 0)
            {
                height = 0.9f;
                RecalculateVertexVectors();
            }
            else
            {
                height = 1;
                RecalculateVertexVectors();
            }
            return base.GetShapeMesh(exposedFace, lightAttenuation);
        }
    }
}