using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Database
{
	[CreateAssetMenu(fileName = "New Game Settings", menuName = "Block2nd/Settings/Game")]
	[Serializable]
	public class GameSettings : ScriptableObject
	{
		public float cameraFov = 60;
		public bool mobileControl;
		public int viewDistance = 16;
		public bool infiniteJump = true;
	}
}