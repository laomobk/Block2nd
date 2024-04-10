using Block2nd.Client;
using UnityEngine;

namespace TitlePage
{
    public class CameraSkyBoxController : MonoBehaviour
    {
        public Material skyBoxMaterial;
        public Texture2D defaultSkyboxTexture;
        
        private void Start()
        {
            if (ClientSharedData.zoz)
            {
                var tex = Resources.Load<Texture2D>("sos/sos_panorama");
                if (tex == null)
                {
                    return;
                }
                
                skyBoxMaterial.SetTexture("_MainTex", tex);
            }
            else
            {
                skyBoxMaterial.SetTexture("_MainTex", defaultSkyboxTexture);
            }
        }
    }
}