using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class PosText : MonoBehaviour
    {
        private Text text;
        private GameObject player;

        // Use this for initialization
        void Start()
        {
            this.text = GetComponent<Text>();
            this.player = GameObject.Find("Player");
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 pos = this.player.transform.position;
            this.text.text = "X: " + (int)pos.x + " Y: " + (int)pos.y + " Z: " + (int)pos.z;
        }
    }
}