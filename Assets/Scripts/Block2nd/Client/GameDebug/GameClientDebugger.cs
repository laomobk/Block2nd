using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Block2nd.Client.GameDebug
{
	public class GameClientDebugger : MonoBehaviour
	{
		[SerializeField] private GameClient client;
		[SerializeField] private GameObject debugObject;
		
		private static GameClientDebugger _instance;

		public static GameClientDebugger Instance => _instance;

		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(_instance);
			}
			_instance = this;
		}

		public GameObject CreateDebugObject(Vector3 position, Color color, string label = "")
		{
			var go = Instantiate(debugObject);

			go.transform.position = position;
			go.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", color);
			go.GetComponent<DebugObject>().label = label;

			return go;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				var coroutine = client.CurrentLevel.GetChunkFromCoords(13, 8)
					.DebugLightUpdate();
				StartCoroutine(coroutine);
			}
		}
	}
}