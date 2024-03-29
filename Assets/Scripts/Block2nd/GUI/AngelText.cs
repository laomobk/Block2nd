using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
	public class AngelText : MonoBehaviour
	{
		private GameObject playerCamera;
		private Text text;
	
		// Use this for initialization
		void Start ()
		{
			playerCamera = GameObject.FindWithTag("MainCamera");
			text = GetComponent<Text>();
		}
	
		// Update is called once per frame
		void Update ()
		{
			text.text = "Angel: " + playerCamera.transform.rotation.eulerAngles.ToString();
		}
	}
}
