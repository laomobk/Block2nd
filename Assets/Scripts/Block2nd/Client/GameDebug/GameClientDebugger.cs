using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Database;
using Block2nd.Utils;
using Block2nd.World;
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
				Debug.Log("DebugLightUpdate");
				var coroutine = client.CurrentLevel.GetChunkPlayerIn()
					.DebugLightUpdate();
				StartCoroutine(coroutine);
			}

			if (Input.GetKeyDown(KeyCode.Backslash))
			{
				var chunk = client.CurrentLevel.GetChunkPlayerIn();
				Debug.Log("force refresh chunk at chunk pos: " + chunk.worldBasePosition.ToChunkCoordPos());
				client.CurrentLevel.ChunkRenderEntityManager.RenderChunk(chunk, true);
			}
		}

		public void ShowPlayerChunkHeightMapBake()
		{
			StartCoroutine(ShowBakeHeightMapIter(client.CurrentLevel.GetChunkPlayerIn()));
		}

		public void ShowPlayerChunkNonFillLightBlock()
		{
			StartCoroutine(ShowPlayerChunkNonFillLightBlockIter(client.CurrentLevel.GetChunkPlayerIn()));
		}

		public void PrintPlayerChunkSkyLightMap()
		{
			var chunk = client.CurrentLevel.GetChunkPlayerIn();
			Debug.Log(ArrayFormat.Format(chunk.skyLightMap));
		}

		private IEnumerator ShowBakeHeightMapIter(Chunk chunk)
		{
			var width = chunk.chunkBlocks.GetLength(0);
			var height = chunk.chunkBlocks.GetLength(1);

			for (int x = 0; x < width; ++x)
			{
				for (int z = 0; z < width; ++z)
				{
					for (int y = height - 1; y >= 0; --y)
					{
						var code = chunk.chunkBlocks[x, y, z].blockCode;
						if (code != 0 && (BlockMetaDatabase.types[code] & BlockTypeBits.PlantBit) == 0)
						{
							Destroy(CreateDebugObject(
									new Vector3(x, y + 1, z) + chunk.worldBasePosition.ToUnityVector3(), 
									Color.magenta), 5f);

							yield return null;
							
							break;
						}
					}
				}
			}
		}
		
		private IEnumerator ShowPlayerChunkNonFillLightBlockIter(Chunk chunk)
		{
			var width = chunk.chunkBlocks.GetLength(0);
			var height = chunk.chunkBlocks.GetLength(1);

			for (int x = 0; x < width; ++x)
			{
				for (int z = 0; z < width; ++z)
				{
					for (int y = 0; y < height; ++y)
					{
						var light = chunk.skyLightMap[chunk.CalcLightMapIndex(x, y, z)];
						
						var code = chunk.chunkBlocks[x, y, z].blockCode;
						if (code != 0 && (BlockMetaDatabase.types[code] & BlockTypeBits.PlantBit) == 0)
							continue;

						if (light < 15)
						{
							Destroy(CreateDebugObject(
								new Vector3(x, y + 1, z) + chunk.worldBasePosition.ToUnityVector3(),
								new Color(light / 15f, light / 15f, 1)), 5f);

							yield return null;
						}
					}
				}
			}
		}
	}
}