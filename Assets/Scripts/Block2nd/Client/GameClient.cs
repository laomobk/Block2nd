using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Database;
using Block2nd.GamePlay;
using Block2nd.GUI;
using Block2nd.GUI.GameGUI;
using Block2nd.World;
using UnityEngine;
using UnityEngine.Serialization;

namespace Block2nd.Client
{
	public enum GameClientState
	{
		MENU,
		GUI,
		GAME
	}
	
	public class GameClient : MonoBehaviour
	{
		public GameObject levelPrefab;
		public GUICanvasManager guiCanvasManager;
		public GameSettings gameSettings;
		public WorldSettings worldSettings;
		public Transform worldTransform;
		public Player player;
		public int initWorldWidth = 256;
		private GameObject currentLevel;
		
		public Material terrainMaterial;
		public Shader[] shaderCandidates;
		public int[] viewDistanceCandidates = new int[]{4, 8, 16, 32};

		private GameClientState gameClientState = GameClientState.GAME;
		private int viewDistanceCandidateIdx = 2;
		private int shaderCandidateIdx = 0;
		private bool cursorLocked;
		private GameSaveManager gameSaveManager;

		public GameClientState GameClientState => gameClientState;

		public Level CurrentLevel => currentLevel.GetComponent<Level>();
		public int ViewDistanceCandidateIdx => viewDistanceCandidateIdx;
		public int ShaderCandidateIdx => shaderCandidateIdx;

		public string GameVersion => "0.1.8.0a";

		public string GameVersionSubject => "NOT STABLE!!";

		public IGameGUI currentGUI;

		private void Awake()
		{
			gameSaveManager = new GameSaveManager(Application.persistentDataPath);
		}

		private void Start()
		{
			worldSettings.levelWidth = initWorldWidth;
			
			if (Application.isMobilePlatform)
			{
				Application.targetFrameRate = 120;
			}

			guiCanvasManager.SetGameVersionText(GameVersion + (" - " + GameVersionSubject));
			
			ClientStart();

			if (!gameSettings.mobileControl && Application.isMobilePlatform)
			{
				gameSettings.mobileControl = true;
			}
			
			guiCanvasManager.mobileUICanvas.SetActive(gameSettings.mobileControl);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.G)) 
			{
				GenerateWorld();
			}
			
			if (Input.GetKeyDown(KeyCode.L)) 
			{
				SwitchShader();
			}
			
			if (Input.GetKeyDown(KeyCode.K)) 
			{
				SwitchDistance();
			}

			if (Input.GetKeyDown(KeyCode.Escape) && !gameSettings.mobileControl)
			{
				switch (gameClientState)
				{
					case GameClientState.GAME:
						OpenMenu();
						gameClientState = GameClientState.MENU;
						break;
					case GameClientState.MENU:
						CloseMenu();
						break;
					case GameClientState.GUI:
						if (currentGUI != null)
						{
							guiCanvasManager.SetGUIBackgroundState(false);
							gameClientState = GameClientState.GAME;
							currentGUI.OnCloseGUI();
							currentGUI = null;
						}
						break;
				}
			}
			
			if (Input.GetKeyDown(KeyCode.E) && !gameSettings.mobileControl)
			{
				if (currentGUI is AllItemUI)
				{
					guiCanvasManager.SetGUIBackgroundState(false);
					gameClientState = GameClientState.GAME;
					currentGUI.OnCloseGUI();
					currentGUI = null;
				}
				else
				{
					OpenAllItems();
				}
			}

			if (!gameSettings.mobileControl)
			{
				cursorLocked = gameClientState == GameClientState.GAME;

				Cursor.visible = !cursorLocked;
				Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
			}

			SetLightingWithGameSetting();
		}

		public void OpenAllItems()
		{
			gameClientState = GameClientState.GUI;
			currentGUI = guiCanvasManager.allItemUI;
			currentGUI.OnOpenGUI();
		}

		public void ToggleSelectItemGUI()
		{
			if (currentGUI is AllItemUI)
			{
				gameClientState = GameClientState.GAME;
				currentGUI.OnCloseGUI();
				currentGUI = null;
			}
			else
			{
				OpenAllItems();
			}
		}

		private void ClientStart()
		{
			SyncGameSettings();
			GenerateWorld(new TestTerrainGenerator(worldSettings));
		}

		private void SetLightingWithGameSetting()
		{
			var viewDistance = gameSettings.viewDistance;

			RenderSettings.fogEndDistance = (viewDistance - 2) * worldSettings.chunkWidth;

			if (RenderSettings.fogEndDistance < 10)
				RenderSettings.fogEndDistance = 10;
		}

		private void SyncGameSettings()
		{
			gameSettings.viewDistance = viewDistanceCandidates[viewDistanceCandidateIdx];
			var shader = shaderCandidates[shaderCandidateIdx];
			terrainMaterial.shader = shader;
		}

		public void OpenMenu()
		{
			gameClientState = GameClientState.MENU;
			guiCanvasManager.gameMenu.OpenFirstPage();
		}

		public void CloseMenu()
		{
			if (gameClientState != GameClientState.GAME)
			{
				gameClientState = GameClientState.GAME;
				guiCanvasManager.gameMenu.CloseMenu();
			}
		}

		public void SaveWorld()
		{
			gameSaveManager.SaveLevel(player, currentLevel.GetComponent<Level>());
		}

		public void LoadWorld()
		{
			var levelName = currentLevel.GetComponent<Level>().levelName;
			var saveData = gameSaveManager.LoadSave(levelName);

			var playerPos = new Vector3(
				saveData.playerPosition[0],
				saveData.playerPosition[1],
				saveData.playerPosition[2]);

			player.transform.position = playerPos;
			player.horAngle = saveData.playerViewHorAngel;
			player.rotAngle = saveData.playerViewRotAngel;
			
			if (currentLevel != null)
			{
				DestroyImmediate(currentLevel);
			}
			
			currentLevel = Instantiate(levelPrefab, worldTransform);
			var level = currentLevel.GetComponent<Level>();

			level.levelName = levelName;
			level.ChunkManager.chunkEntries = new List<ChunkEntry>();

			var entries = level.ChunkManager.chunkEntries;

			for (int i = 0; i < saveData.chunkEntries.Length; ++i)
			{
				var basePos = saveData.chunkEntries[i].basePos;
				var chunk = level.ChunkManager.AddNewChunkGameObject(basePos.x, basePos.z);
				entries.Add(new ChunkEntry
				{
					chunk = chunk,
					basePos = basePos
				});
			}

			StartCoroutine(level.ChunkManager.ChunkManagementWorkerCoroutine());
		}

		public void GenerateWorld(TerrainGenerator terrainGenerator = null)
		{
			CloseMenu();
			
			if (currentLevel != null)
			{
				DestroyImmediate(currentLevel);
			}
			
			currentLevel = Instantiate(levelPrefab, worldTransform);
			
			StartCoroutine(currentLevel.GetComponent<Level>().CreateLevelCoroutine(terrainGenerator));
		}

		public Level GetCurrentLevel()
		{
			return currentLevel != null ? currentLevel.GetComponent<Level>() : null;
		}

		public int SwitchDistance()
		{
			if (viewDistanceCandidateIdx < viewDistanceCandidates.Length)
			{
				viewDistanceCandidateIdx = (viewDistanceCandidateIdx + 1) % viewDistanceCandidates.Length;
				gameSettings.viewDistance = viewDistanceCandidates[viewDistanceCandidateIdx];
			}
			
			GetCurrentLevel().ChunkManager.SortChunksByDistance(player.transform.position, 
				viewDistanceCandidateIdx == 0);  // 当从远视距转到进视距时反转排序顺序
			GetCurrentLevel().ChunkManager.ForceBeginChunksManagement();

			return viewDistanceCandidateIdx;
		}

		public int SwitchShader()
		{
			if (shaderCandidateIdx < shaderCandidates.Length)
			{
				shaderCandidateIdx = (shaderCandidateIdx + 1) % shaderCandidates.Length;
				var shader = shaderCandidates[shaderCandidateIdx];
				terrainMaterial.shader = shader;
			}

			return shaderCandidateIdx;
		}
	}
}