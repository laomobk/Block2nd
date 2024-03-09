using UnityEngine;

namespace Block2nd.Render
{
    public class CameraPostProcessing : MonoBehaviour
    {
        public bool enableBloom;
    
        public Material bloomPostMaterial;

        public int bloomDownSample = 2;

        public int blurIteration = 3;

        private void Awake()
        {
            if (Application.isMobilePlatform)
                Application.targetFrameRate = 120;
            
            GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        }

        private void _BloomPost(RenderTexture src, RenderTexture dest)
        {
            var rtHeight = src.height / bloomDownSample;
            var rtWidth = src.height / bloomDownSample;
        
            var buffer0 = RenderTexture.GetTemporary(rtHeight, rtWidth);
            var bufferBloom = RenderTexture.GetTemporary(rtHeight, rtWidth);
            buffer0.filterMode = FilterMode.Bilinear;
            bufferBloom.filterMode = FilterMode.Bilinear;
        
            Graphics.Blit(src, buffer0, bloomPostMaterial, 0);

            for (int i = 0; i < blurIteration; ++i)
            {
                bloomPostMaterial.SetFloat("_PixelOffset", (float)i / bloomDownSample);
            
                if (i % 2 == 0)
                {
                    Graphics.Blit(buffer0, bufferBloom, bloomPostMaterial, 1);
                }
                else
                {
                    Graphics.Blit(bufferBloom, buffer0, bloomPostMaterial, 1);
                }
            }

            bloomPostMaterial.SetTexture("_OriginalTex", src);
            Graphics.Blit(bufferBloom, dest, bloomPostMaterial, 2);

            RenderTexture.ReleaseTemporary(buffer0);
            RenderTexture.ReleaseTemporary(bufferBloom);
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (enableBloom)
                _BloomPost(src, dest);
            else
                Graphics.Blit(src, dest);
        }
    }
}
