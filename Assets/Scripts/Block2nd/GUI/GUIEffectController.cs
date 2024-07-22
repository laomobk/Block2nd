using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class GUIEffectController : MonoBehaviour
    {
        [SerializeField] private Image vignetteEffectImage;

        public void SetVignetteEffectStrength(float strength)
        {
            vignetteEffectImage.color = new Color(0, 0, 0, strength);
        }
    }
}