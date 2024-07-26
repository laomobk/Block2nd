using System;
using System.IO;
using Block2nd.GameSave;
using Block2nd.Persistence.KNBT;
using UnityEngine;

namespace Block2nd.Scriptable
{
	[CreateAssetMenu(fileName = "New Game Settings", menuName = "Block2nd/Settings/Game")]
	[Serializable]
	public class GameSettings : ScriptableObject
	{
		public bool dirty = false;
		
		// 为了运行效率，故不使用访问器，dirty 字段需要即使修改 !
		public float cameraFov = 60;
		public bool mobileControl;
		public int viewDistance = 16;
		public bool infiniteJump = true;
		public bool ambientOcclusion = true;
		public bool fog = true;
		public bool music = true;
		public int shader = 0;

		public bool vSync = true;

		public void LoadSettings()
		{
			var root = GameRootDirectory.GetInstance();
			var fp = Path.Combine(root.gameDataRoot, "Settings.dat");
			
			var kmbt = new KNBTTagCompound("Settings");

			if (File.Exists(fp))
			{
				var reader = new BinaryReader(new FileStream(fp, FileMode.Open, FileAccess.Read));
				kmbt.Read(reader);
				reader.Dispose();
			}

			cameraFov = kmbt.GetFloat("CameraFov", 70);
			viewDistance = kmbt.GetInt("ViewDistance", 8);
			music = kmbt.GetBoolean("Music", true);
			shader = kmbt.GetInt("Shader", 0);
			vSync = kmbt.GetBoolean("vSync", true);
			
			Debug.Log("Game settings loaded");
		}
		
		public void StoreSettings()
		{
			var root = GameRootDirectory.GetInstance();
			var fp = Path.Combine(root.gameDataRoot, "Settings.dat");
			
			var writer = new BinaryWriter(new FileStream(fp, FileMode.OpenOrCreate, FileAccess.Write));
			var kmbt = new KNBTTagCompound("Settings");
			
			kmbt.SetFloat("CameraFov", cameraFov);
			kmbt.SetInt("ViewDistance", viewDistance);
			kmbt.SetByte("Music", music);
			kmbt.SetInt("Shader", shader);
			kmbt.SetByte("vSync", vSync);
			
			kmbt.Write(writer);
			
			writer.Dispose();
		}
	}
}