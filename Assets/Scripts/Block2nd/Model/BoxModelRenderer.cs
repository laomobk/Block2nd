using Block2nd.Database.Meta;
using UnityEngine;

namespace Block2nd.Model
{
    public class BoxModelRenderer: ModelRenderer
    {
        private Vector3 offset;
        private float height, width, depth;

        private float texX, texY, uvWidth, uvHeight, uvDepth;

        private BlockMesh bakedMesh;
        
        private Vector3 original;
        private Vector3 forward;
        private Vector3 up;
        private Vector3 right;

        private Vector3 BBR;
        private Vector3 BTR;
        private Vector3 BBL;
        private Vector3 BTL;
        private Vector3 FBR;
        private Vector3 FBL;
        private Vector3 FTR;
        private Vector3 FTL;

        public BoxModelRenderer(float depth, float width, float height, 
                                float texX, float texY, float uvDepth, float uvWidth, float uvHeight,
                                Vector3 offset, Vector3 localPos): base(localPos)
        {
            this.offset = offset;
            this.height = height;
            this.width = width;
            this.depth = depth;
            this.texX = texX;
            this.texY = texY;
            this.uvDepth = uvDepth;
            this.uvWidth = uvWidth;
            this.uvHeight = uvHeight;
            
            RecalculateVertexVectors();
            
            BakeBoxMesh();
        }

        private void RecalculateVertexVectors()
        {
            forward = new Vector3(0, 0, depth);
            up = new Vector3(0, height, 0);
            right = new Vector3(width, 0, 0);

            forward += offset;
            up += offset;
            right += offset;

            BBR = original;  // B(ottom) B(ack) R(ight)
            BBL = original + right;
            BTR = original + up;
            BTL = original + up + right;
            
            FBR = original + forward;
            FBL = FBR + right;
            FTR = FBR + up;
            FTL = FBR + up + right;
        }

        private void BakeBoxMesh()
        {
            var positions = new Vector3[24]; 
            var texcoords = new Vector2[24];
            var indices = new int[36];
            
            bakedMesh = new BlockMesh();

            byte posIdx = 0, texcoordsIdx = 0, trianglesIdx = 0;
            
            QuadMeshHelper.AppendQuadVertices(
                ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                BBL, up, forward, 
                texX, texY, uvDepth, uvHeight, 
                positions, texcoords, indices);  // X+
            QuadMeshHelper.AppendQuadVertices(
                ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                FBL, up, -right, 
                texX + uvDepth, texY, uvWidth, uvHeight, 
                positions, texcoords, indices);  // Z+
            QuadMeshHelper.AppendQuadVertices(
                ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                FBR, up, -forward, 
                texX + uvDepth + uvWidth, texY, uvDepth, uvHeight, 
                positions, texcoords, indices);  // X-
            QuadMeshHelper.AppendQuadVertices(
                ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                BBR, up, right, 
                texX + 2 * uvDepth + uvWidth, texY, uvWidth, uvHeight, 
                positions, texcoords, indices);  // Z-
            QuadMeshHelper.AppendQuadVertices(
                ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                FTL, -forward, -right, 
                texX + uvDepth, texY + uvHeight, uvDepth, uvHeight, 
                positions, texcoords, indices);  // Y+
            QuadMeshHelper.AppendQuadVertices(
                ref posIdx, ref texcoordsIdx, ref trianglesIdx,
                BBL, forward, -right, 
                texX + uvDepth + uvWidth, texY + uvHeight, uvDepth, uvHeight, 
                positions, texcoords, indices);  // Y-

            bakedMesh.positions = positions;
            bakedMesh.triangles = indices;
            bakedMesh.texcoords = texcoords;
        }

        public override BlockMesh GetBlockMesh()
        {
            return bakedMesh;
        }
    }
}