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
using Block2nd.Scriptable;
using Block2nd.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = System.Random;

namespace Block2nd.Client
{
	public enum GameClientState
	{
		MENU,
		GUI,
		GAME,
		CHAT,
		IN_PROGRESS
	}
	
	public class GameClient : MonoBehaviour
	{
		[SerializeField] private GameObject levelPrefab;
		[SerializeField] private GUICanvasManager guiCanvasManager;
		[SerializeField] private Player player;
		[SerializeField] private GUIEffectController guiEffectController;
		
		public GUIEffectController GuiEffectController => guiEffectController;
		public GUICanvasManager GuiCanvasManager => guiCanvasManager;
		public Player Player => player;
		
		public GameSettings gameSettings;
		public WorldSettings worldSettings;
		public Transform worldTransform;
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
		private static GameClient _instance;

		public static GameClient Instance => _instance;

		private void OnApplicationQuit()
		{
			StartCoroutine(SaveAndQuitCoroutine(false));
		}

		private void Awake()
		{
			if (_instance != null)
			{
				DestroyImmediate(_instance);
			}

			_instance = this;

			commandRuntime = new CommandRuntime(this);
		}

		private void OnDestroy()
		{
			_instance = null;
		}

		private void Start()
		{
			worldSettings.levelWidth = initWorldWidth;
			
			if (Application.isMobilePlatform)
			{
				Application.targetFrameRate = 120;
			}

			guiCanvasManager.SetGameVersionText("Block2nd " + GameVersion.Subtitle + " " + GameVersion.Version);
			
			SyncClientSettings();
			ClientStart();

			if (!gameSettings.mobileControl && Application.isMobilePlatform)
			{
				gameSettings.mobileControl = true;
			}
			
			guiCanvasManager.mobileUICanvas.SetActive(gameSettings.mobileControl);
		}

		private void SyncClientSettings()
		{
			gameSettings.LoadSettings();
			gameSettings.dirty = false;
			SetLightingWithGameSetting();
			terrainMaterial.shader = shaderCandidates[gameSettings.shader];
			GlobalMusicPlayer.SetVolume(gameSettings.music ? 1 : 0);
			QualitySettings.vSyncCount = gameSettings.vSync ? 1 : 0;
		}

		private void Update()
		{
			// for (int i = 0; i < 50000000; ++i){}
			
			if (gameClientState == GameClientState.GAME)
			{
				if (Input.GetKeyDown(KeyCode.L))
				{
					SwitchShader();
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

				if (Input.GetKeyDown(KeyCode.F3))
				{
					guiCanvasManager.ToggleDebugUI();
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

			if (gameSettings.dirty)
			{
				SyncClientSettings();
			}
			
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

		public bool TryGetCurrentLevel(out Level level)
		{
			level = currentLevel != null ? currentLevel.GetComponent<Level>() : null;

			return level != null;
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
				RenderSettings.fogDensity = 0.003f;
				return;
			}
			
			if (viewDistance > 7)
			{
				RenderSettings.fogDensity = 0.01f;
				return;
			}
			
			if (viewDistance > 0)
			{
				RenderSettings.fogDensity = 0.025f;
				return;
			}
		}

		private void SyncGameSettings()
		{
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
			var preview = ClientSharedData.levelSavePreviewInLastContext;
			
			if (preview == null)
			{
				EnterWorld(new LevelSavePreview
				{
					folderName = "Level_01",
					name = "Level_01",
					seed = 0,
					newWorld = false,
					levelTime = 540,
				});
				return;
			}
			
			worldSettings.seed = preview.seed;

			LevelSaveHandler handler = new LevelSaveHandler(preview.folderName);
			EnterWorld(preview, 
				new ChunkProviderGenerateOrLoad(
					new LocalChunkLoaderSingleChunk(), BuiltinChunkGeneratorFactory.GetChunkGeneratorFromId(
						preview.terrainType, worldSettings)), handler);
		}

		private void EnterWorld(LevelSavePreview preview,
			IChunkProvider chunkProvider = null, LevelSaveHandler saveHandler = null)
		{
			
			StartCoroutine(EnterWorldCoroutine(preview, chunkProvider, saveHandler));
		}

		private IEnumerator EnterWorldCoroutine(LevelSavePreview preview,
			IChunkProvider chunkProvider = null, LevelSaveHandler saveHandler = null)
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

			level.seed = preview.seed;
			level.random = new System.Random(preview.seed);
			level.levelTime = preview.levelTime;

			if (chunkProvider != null)
				level.SetChunkProvider(chunkProvider);

			if (saveHandler == null)
			{
				saveHandler = new LevelSaveHandler("Level_01");
			} 
			
			level.levelSaveHandler = saveHandler;
			level.levelName = preview.name;
			level.levelFolderName = preview.folderName;

			if (preview.newWorld)
				level.PrepareLevel();
			
			progressUI.SetTitle("Spawning player...");
			progressUI.SetProgress("");
			yield return null;

			Vector3 point;

			if (RecoveryPlayer(saveHandler) && !preview.newWorld)
			{
				point = player.transform.position;
			}
			else
			{
				point = SpawnPlayer(level);
			}

			yield return StartCoroutine(level.ProvideChunksSurroundingCoroutineWithReport(point));
			
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
			
			reader.Dispose();

			return true;
		}

		public Vector3 SpawnPlayer(Level level)
		{
			Random random = new Random();

			Vector3 point;
			
			var ofs = new Vector3(0.10727f, 0, 0.10727f);
			var count = 0;
			var maxCount = 15;

			int code;

			do
			{
				point = new Vector3(random.Next(-300, 300), 0, random.Next(-300, 300));

				level.ProvideChunksSurrounding(point, renderImmediately: false, waitForProviding: true, radius: 1);
				point.y = level.GetHeight((int) point.x, (int) point.z);

				code = level.GetBlock(point).blockCode;

			} while (count++ < maxCount && 
			         code != BlockMetaDatabase.BuiltinBlockCode.Grass &&
			         code != BlockMetaDatabase.BuiltinBlockCode.Sand);

			point += ofs;
			point.y += 3;
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
			StartCoroutine(SaveAndQuitCoroutine(true));
		}

		public IEnumerator SaveAndQuitCoroutine(bool jumpToTitle)
		{
			CloseMenu();
			
			gameClientState = GameClientState.IN_PROGRESS;

			yield return null;
			
			var progressUI = guiCanvasManager.worldGeneratingProgressUI;
			
			progressUI.gameObject.SetActive(true);
			
			progressUI.SetTitle("Saving World...");
			progressUI.SetProgress("");
			yield return null;
			
			CurrentLevel.StopAllCoroutines();
			SaveLevel();
			CurrentLevel.UnloadLevel();

			Resources.UnloadUnusedAssets();
			
			GC.Collect();
			
			if (jumpToTitle)
				SceneManager.LoadScene("Title");
		}
	}
}