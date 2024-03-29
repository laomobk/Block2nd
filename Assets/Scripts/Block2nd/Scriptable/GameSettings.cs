using System;
using UnityEngine;

namespace Block2nd.Scriptable
{
	[CreateAssetMenu(fileName = "New Game Settings", menuName = "Block2nd/Settings/Game")]
	[Serializable]
	public class GameSettings : ScriptableObject
	{
		public float cameraFov = 60;
		public bool mobileControl;
		public int viewDistance = 16;
		public bool infiniteJump = true;
		public bool ambientOcclusion = true;
		public bool fog = true;
	}
}