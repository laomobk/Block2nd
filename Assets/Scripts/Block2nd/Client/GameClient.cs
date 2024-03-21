using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.CommandLine;
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
		GAME,
		CHAT,
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

		private CommandRuntime commandRuntime;

		public GameClientState GameClientState => gameClientState;

		public Level CurrentLevel => currentLevel.GetComponent<Level>();
		public int ViewDistanceCandidateIdx => viewDistanceCandidateIdx;
		public int ShaderCandidateIdx => shaderCandidateIdx;

		public string GameVersion => "0.2.0a";

		public string GameVersionSubject => "Infdev";

		public IGameGUI currentGUI;

		private int playerTickCount = 0;

		private void Awake()
		{
			commandRuntime = new CommandRuntime(this);
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
			if (gameClientState == GameClientState.GAME)
			{
				if (Input.GetKeyDown(KeyCode.G))
				{
					EnterWorld();
				}

				if (Input.GetKeyDown(KeyCode.L))
				{
					SwitchShader();
				}

				if (Input.GetKeyDown(KeyCode.K))
				{
					SwitchDistance();
				}

				if (Input.GetKeyDown(KeyCode.Slash))
				{
					gameClientState = GameClientState.CHAT;
					SetChatUIState(true, "/");
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
			}

			if (gameClientState == GameClientState.CHAT)
			{
				if (Input.GetKeyDown(KeyCode.Return))
				{
					ClientSendMessage(guiCanvasManager.chatUI.GetInputText());
					SetChatUIState(false);
					gameClientState = GameClientState.GAME;
				}
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
					case GameClientState.CHAT:
						SetChatUIState(false);
						gameClientState = GameClientState.GAME;
						break;
				}
			}

			if (!gameSettings.mobileControl)
			{
				cursorLocked = gameClientState == GameClientState.GAME;

				Cursor.visible = !cursorLocked;
				Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
			}

			SetLightingWithGameSetting();
			
			ClientTick();
		}
		
		private void ClientStart()
		{
			SyncGameSettings();
			EnterWorld(/* new TestTerrainNoiseGenerator(worldSettings) */);
		}

		private void ClientTick()
		{
			
		}

		public void ClientSendMessage(string message)
		{
			if (message.StartsWith("/"))
			{
				var err = commandRuntime.ExecuteCommandRaw(message);
				if (err != null)
				{
					UnityEngine.Debug.Log(err.ToString());
				}
			}
			else
			{
				UnityEngine.Debug.Log("Send message: " + message);
			}
		}

		private void SetLightingWithGameSetting()
		{
			var viewDistance = gameSettings.viewDistance;

			RenderSettings.fogEndDistance = (viewDistance - 2) << 4;

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

		public void EnterWorld(TerrainNoiseGenerator terrainNoiseGenerator = null)
		{
			CloseMenu();

			var progressUI = guiCanvasManager.worldGeneratingProgressUI;
			
			progressUI.gameObject.SetActive(true);
			progressUI.SetTitle("Generating terrain...");
			progressUI.SetProgress("");
			
			if (currentLevel != null)
			{
				DestroyImmediate(currentLevel);
			}
			
			currentLevel = Instantiate(levelPrefab, worldTransform);
			var level = currentLevel.GetComponent<Level>();
			
			var point = SpawnPlayer(level);
			
			progressUI.gameObject.SetActive(false);
			
			StartCoroutine(level.RenderChunksSurrounding(point));
			
			StartCoroutine(level.LevelTickCoroutine());
		}

		public Vector3 SpawnPlayer(Level level)
		{
			var point = new Vector3(0.10727f, 0, 0.10727f);
			level.ProvideChunksSurrounding(point, renderImmediately: false, waitForProviding: true);
			point.y = level.GetHeight((int) point.x, (int) point.z) + 3;
			
			player.ResetPlayer(point);

			return point;
		}

		public Level GetCurrentLevel()
		{
			return currentLevel != null ? currentLevel.GetComponent<Level>() : null;
		}

		public void SetChatUIState(bool state, string initText = "")
		{
			guiCanvasManager.chatUI.gameObject.SetActive(state);
			
			if (state)
			{
				guiCanvasManager.chatUI.Clear();
				guiCanvasManager.chatUI.Focus();
				guiCanvasManager.chatUI.Set(initText);
			}
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