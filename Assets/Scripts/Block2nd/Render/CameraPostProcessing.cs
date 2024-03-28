using UnityEngine;

namespace Block2nd.Render
{
    public class CameraPostProcessing : MonoBehaviour
    {
        public Material material;

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, material);
        }
    }
}
