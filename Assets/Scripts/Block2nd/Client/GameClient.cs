using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.CommandLine;
using Block2nd.Database;
using Block2nd.GamePlay;
using Block2nd.GameSave;
using Block2nd.GUI;
using Block2nd.GUI.GameGUI;
using Block2nd.Persistence.KNBT;
using Block2nd.Phys;
using Block2nd.World;
using UnityEngine;
using UnityEngine.SceneManagement;
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
		private Coroutine levelTickCoroutine;
		
		public Material terrainMaterial;
		public Shader[] shaderCandidates;
		public int[] viewDistanceCandidates = new int[]{4, 8, 16, 32};

		private GameClientState gameClientState = GameClientState.GAME;
		private int viewDistanceCandidateIdx = 0;
		private int shaderCandidateIdx = 0;
		private bool cursorLocked;

		private CommandRuntime commandRuntime;

		public GameClientState GameClientState => gameClientState;

		public Level CurrentLevel => currentLevel != null ? currentLevel.GetComponent<Level>() : null;
		public int ViewDistanceCandidateIdx => viewDistanceCandidateIdx;
		public int ShaderCandidateIdx => shaderCandidateIdx;

		public IGameGUI currentGUI;

		private int playerTickCount = 0;

		private void Awake()
		{
			commandRuntime = new CommandRuntime(this);
		}

		private void Start()
		{
			worldSettings.levelWidth = initWorldWidth;
			
			if (Application.isMobilePlatform)
			{
				Application.targetFrameRate = 120;
			}

			guiCanvasManager.SetGameVersionText("Block2nd " + GameVersion.Subtitle + " " + GameVersion.Version);
			
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
					EnterWorldCoroutine();
				}

				if (Input.GetKeyDown(KeyCode.L))
				{
					SwitchShader();
				}

				if (Input.GetKeyDown(KeyCode.K))
				{
					SwitchDistance();
				}

				if (Input.GetKeyDown(KeyCode.F))
				{
					SetFogState(!RenderSettings.fog);
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
			StartCoroutine(GlobalMusicPlayer.BGMPlayCoroutine());
			
			CheckAndEnterWorld();
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

			if (viewDistance > 31)
			{
				RenderSettings.fogDensity = 0.001f;
				return;
			}
			
			if (viewDistance > 15)
			{
				RenderSettings.fogDensity = 0.0025f;
				return;
			}
			
			if (viewDistance > 7)
			{
				RenderSettings.fogDensity = 0.005f;
				return;
			}
			
			if (viewDistance > 0)
			{
				RenderSettings.fogDensity = 0.02f;
				return;
			}
		}

		private void SyncGameSettings()
		{
			gameSettings.viewDistance = viewDistanceCandidates[viewDistanceCandidateIdx];
			var shader = shaderCandidates[shaderCandidateIdx];
			terrainMaterial.shader = shader;
			SetFogState(gameSettings.fog);
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

		public void CheckAndEnterWorld()
		{
			var preview = ClientSharedData.enterWorldLevelSavePreview;

			if (preview == null)
			{
				EnterWorld();
				return;
			}

			LevelSaveHandler handler = new LevelSaveHandler(preview.folderName);
			EnterWorld(
				new ChunkProviderGenerateOrLoad(
					new LocalChunkLoader(), BuiltinChunkGeneratorFactory.GetChunkGeneratorFromId(
						preview.terrainType, worldSettings)),
				handler, preview.name, preview.folderName);
		}

		public void EnterWorld(
			IChunkProvider chunkProvider = null, LevelSaveHandler saveHandler = null,
			string worldName = "Level_01", string worldFolderName = "Level_01")
		{
			StartCoroutine(EnterWorldCoroutine(chunkProvider, saveHandler, worldName, worldFolderName));
		}

		private IEnumerator EnterWorldCoroutine(
			IChunkProvider chunkProvider = null, LevelSaveHandler saveHandler = null,
			string worldName = "Level_01", string worldFolderName = "Level_01")
		{
			CloseMenu();

			var progressUI = guiCanvasManager.worldGeneratingProgressUI;
			
			progressUI.gameObject.SetActive(true);
			
			if (levelTickCoroutine != null)
				StopCoroutine(levelTickCoroutine);
			
			if (currentLevel != null)
			{
				progressUI.SetTitle("Exiting World...");
				progressUI.SetProgress("");
				yield return null;
				
				DestroyImmediate(currentLevel);
			}
			
			progressUI.SetTitle("Creating World...");
			progressUI.SetProgress("");

			yield return null;
			
			currentLevel = Instantiate(levelPrefab, worldTransform);
			var level = currentLevel.GetComponent<Level>();

			if (chunkProvider != null)
				level.SetChunkProvider(chunkProvider);

			if (saveHandler != null)
			{
				level.levelSaveHandler = saveHandler;
			}

			level.levelName = worldName;
			level.levelFolderName = worldFolderName;

			// level.PrepareLevel();
			
			progressUI.SetTitle("Spawning player...");
			progressUI.SetProgress("");
			yield return null;

			Vector3 point;

			if (saveHandler != null)
			{
				if (RecoveryPlayer(saveHandler))
				{
					point = player.transform.position;
				}
				else
				{
					point = SpawnPlayer(level);
				}
			}
			else
			{
				point = SpawnPlayer(level);
			}

			yield return StartCoroutine(level.ProvideChunksSurroundingCoroutineWithReport(point, 6));
			
			progressUI.gameObject.SetActive(false);
			
			levelTickCoroutine = StartCoroutine(level.LevelTickCoroutine());
			
			level.SavePlayerData();
			level.SaveLevelData();
		}

		public bool RecoveryPlayer(LevelSaveHandler saveHandler)
		{
			var reader = saveHandler.GetPlayerDataReader();
			if (reader is null)
				return false;

			var knbt = new KNBTTagCompound("Player");
			knbt.Read(reader);
			
			player.SetPlayerWithKNBTData(knbt);

			return true;
		}

		public Vector3 SpawnPlayer(Level level)
		{
			var point = new Vector3(0.10727f, 0, 0.10727f);
			level.ProvideChunksSurrounding(point, renderImmediately: false, waitForProviding: true, radius: 2);
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

			GetCurrentLevel().breakChunkRender = true;

			return viewDistanceCandidateIdx;
		}

		public void SetFogState(bool state)
		{
			RenderSettings.fog = state;
			gameSettings.fog = state;
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

		public void SaveLevel()
		{
			currentLevel.GetComponent<Level>().SaveLevelCompletely();
		}

		public void SaveAndQuitToTitle()
		{
			SaveLevel();
			SceneManager.LoadScene("Title");
		}
	}
}