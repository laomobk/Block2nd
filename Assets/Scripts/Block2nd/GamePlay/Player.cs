﻿using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.Client.GameDebug;
using Block2nd.GUI;
using Block2nd.Render;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.Entity;
using Block2nd.MathUtil;
using Block2nd.Persistence.KNBT;
using Block2nd.Phys;
using Block2nd.World;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using LightType = Block2nd.World.LightType;

namespace Block2nd.GamePlay
{
    public class Player : MonoBehaviour
    {
        private System.Random random = new System.Random();
        
        private CharacterController controller;
        private GameClient gameClient;

        private RayHit raycastBlockHit = null;

        private Inventory inventory = new Inventory();
        private int holdingBlockCode = 1;

        public float horAngle = 0f;
        public float rotAngle = 0.0f;
        private Vector3 prevPlayerPos = new Vector2(0, 1);

        private float mouseSpeed = 25f;

        private ChunkBlockData selectedBlock;
        private SelectBox selectBox;
        
        private Vector3 dampCameraMotionVelocity = Vector3.zero;
        private Vector3 dampCameraBobblingVelocity = Vector3.zero;

        [HideInInspector] public PlayerController playerController;

        public Camera playerCamera;
        public LayerMask raycastLayerMask;

        public HoldingBlockPreview holdingBlockPreview;
        private float bobbingTime = 0f;

        public Inventory Inventory => inventory;

        public Vector3 Position => transform.position;

        [HideInInspector] public PlayerEntityBase entityBase;
        
        public IntVector3 IntPosition
        {
            get
            {
                var pos = transform.position;
                return new IntVector3(
                    Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            }
        }

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
            entityBase = GetComponent<PlayerEntityBase>();
        }
        
        private void Start()
        {
            SetHoldingBlock(1);

            selectBox = GameObject.FindWithTag("SelectBox").GetComponent<SelectBox>();
            controller = GetComponent<CharacterController>();
            transform.position = new Vector3(15, 13, 3);

            if (!gameClient.gameSettings.mobileControl)
                UpdateRaycast();

            UpdateHoldingItemNameText();
        }

        void Update()
        {
            if (!gameClient.gameSettings.mobileControl)
                UpdateRaycast();
            
            selectBox.gameObject.SetActive(raycastBlockHit != null);
            
            gameClient.GuiCanvasManager.inventoryUI.RenderInventory(inventory);

            if (gameClient.GameClientState == GameClientState.GAME)
            {
                var (ix, iy, iz) = IntPosition;
                var level = gameClient.CurrentLevel;

                if (level)
                {
                    var skyLight = gameClient.CurrentLevel.GetSkyLight(
                        ix, iy, iz, true);
                    var blockLight = gameClient.CurrentLevel.GetBlockLight(
                        ix, iy, iz, true);
                    
                    holdingBlockPreview.SetEnvLight(skyLight / 15f, blockLight / 15f);
                }
            }
            
            if (gameClient.GameClientState == GameClientState.GAME && playerController.OnGround)
                bobbingTime += Time.deltaTime;

            if (gameClient.GameClientState == GameClientState.GAME && !gameClient.gameSettings.mobileControl)
            {
                HandleFunctionalKey();
                HandleItemKey();
                
                if (Input.GetMouseButtonDown(0))
                {
                    if (raycastBlockHit != null)
                    {
                        DestroyBlock(raycastBlockHit);
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (raycastBlockHit != null)
                    {
                        PlaceBlock(raycastBlockHit);
                    }
                }

                var x = Input.GetAxis("Mouse X") * 0.1f;
                var y = Input.GetAxis("Mouse Y") * 0.1f;

                HandleViewRotation(x, y);
            }

            float zAngle;
            ApplyViewBobbing(transform.localPosition + Vector3.up * 0.8f, out var camPos, out zAngle);

            playerCamera.transform.localPosition = Vector3.SmoothDamp(playerCamera.transform.localPosition,
                                                                camPos,
                                                                ref dampCameraMotionVelocity, 0.02f);
            
            var localEulerAngles = playerCamera.transform.localEulerAngles;
            localEulerAngles.z = zAngle;
            playerCamera.transform.localEulerAngles = localEulerAngles;
        }

        public void ResetPlayer()
        {
            gameClient.SpawnPlayer(gameClient.CurrentLevel);
        }

        public void RandomTeleportPlayer()
        {
            var levelWidth = gameClient.worldSettings.levelWidth;

            var x = random.Next(levelWidth / 2 - levelWidth / 4, levelWidth / 2 + levelWidth / 4);
            var z = random.Next(levelWidth / 2 - levelWidth / 4, levelWidth / 2 + levelWidth / 4);
            
            gameClient.CurrentLevel.ChunkManager.BakeAllChunkHeightMap();
            
            var chunk = gameClient.CurrentLevel.GetChunkFromCoords(x >> 4, z >> 4);
            var chunkLocalPos = chunk.WorldToLocal(x, z);
            
            var y = chunk.heightMap[chunkLocalPos.x, chunkLocalPos.z] + 5;
            
            ResetPlayer(new Vector3(x, y, z));
        }

        public void ResetPlayer(Vector3 position)
        {
            gameClient.CurrentLevel.ChunkManager.SortChunksByDistance(
                                                    new Vector3(position.x, 0, position.z));
            gameClient.CurrentLevel.ChunkManager.ForceBeginChunksManagement();
            
            transform.position = position;
            playerController.playerSpeed = Vector3.zero;
            playerController.ResetVelocity();
            
            // entityBase.ResetVelocity();
            entityBase.MoveAABBToWorldPosition();
        }

        private void HandleItemKey()
        {
            var wheel = Input.GetAxis("Mouse ScrollWheel");

            if (wheel > 0)
            {
                SetPrevItem();
            }
            else if (wheel < 0)
            {
                SetNextItem();
            }
        }

        private void UpdateHoldingItemNameText()
        {
            var holdingBlockMeta = BlockMetaDatabase.GetBlockMetaByCode(holdingBlockCode);
            if (holdingBlockMeta != null)
            {
                gameClient.GuiCanvasManager.SetGUIItemNameText(
                    holdingBlockMeta.blockName + " (" + holdingBlockMeta.blockId + ")");
            }
        }

        public void SetPrevItem()
        {
            var code = inventory.blockCodes[inventory.Prev()];
            SetHoldingBlock(BlockMetaDatabase.GetBlockMetaByCode(code));
        }

        public void SetNextItem()
        {
            var code = inventory.blockCodes[inventory.Next()];
            SetHoldingBlock(BlockMetaDatabase.GetBlockMetaByCode(code));
        }

        public void UpdateHoldingItem()
        {
            var code = inventory.blockCodes[inventory.selectIdx];
            SetHoldingBlock(BlockMetaDatabase.GetBlockMetaByCode(code));
        }

        public void SetHoldingBlock(BlockMeta meta)
        {
            if (meta == null)
                return;

            holdingBlockPreview.SetMeshFromMeta(meta);   
            
            holdingBlockCode = meta.blockCode;

            UpdateHoldingItemNameText();
        }

        public void SetHoldingBlock(int blockCode)
        {
            var meta = BlockMetaDatabase.GetBlockMetaByCode(blockCode);
            if (meta == null)
                return;
            SetHoldingBlock(meta);
        }

        public void PlaceBlock(RayHit hit)
        {
            if (hit == null)
                return;

            var level = gameClient.GetCurrentLevel();

            Chunk cp;
            var defaultAction = BlockMetaDatabase.GetBlockBehaviorByCode(level.GetBlock(
                    hit.blockX, 
                    hit.blockY, 
                    hit.blockZ, out cp).blockCode)
                        .OnInteract(hit.ToIntVector3(), level, cp, this);

            if (!defaultAction)
                return;
            
            var intPos = hit.ToNormalAlongIntVector3();
            
            BlockMetaDatabase.GetBlockMetaByCode(holdingBlockCode).behavior.OnBeforePlace(
                ref intPos, level, cp, this);
            
            #if UNITY_EDITOR
            {
                var chk = level.GetChunkFromCoords(intPos.x >> 4, intPos.z >> 4, true);
                var localPos = chk.WorldToLocal(intPos.x, intPos.y, intPos.z);
                Debug.Log(
                    $"skylight: {chk.GetSkyLight(localPos.x, localPos.y, localPos.z)} " +
                    $"blocklight: {chk.GetBlockLight(localPos.x, localPos.y, localPos.z)}");
            }
            #endif

            level.SetBlock(holdingBlockCode, intPos.x, intPos.y, intPos.z, true);
            
            BlockMetaDatabase.GetBlockMetaByCode(holdingBlockCode).behavior.OnAfterPlace(
                intPos, level, cp, this);
            
            holdingBlockPreview.PlayUseBlockAnimation();
        }

        public void DestroyBlock(RayHit hit)
        {
            if (hit == null)
                return;
            gameClient.CurrentLevel.DestroyBlock(hit.blockX, hit.blockY, hit.blockZ);
            holdingBlockPreview.PlayUseBlockAnimation();
        }

        private void ApplyViewBobbing(Vector3 pos, out Vector3 posOfs, out float zAngle)
        {
            if (gameClient.GameClientState == GameClientState.GAME && playerController.PlayerEntityBase.OnGround)
            {
                var speedVector = playerController.playerSpeed;
                speedVector.y = 0;
                var vLength = Mathf.Min(speedVector.magnitude, playerController.GetSpeedRatio());

                var t = bobbingTime * 2.5f + (1 + vLength / 2.5f);
                var bobbing = new Vector3(Mathf.Sin(-3.14159f * t) * 
                                          Mathf.Min(1, vLength / 5f) * 0.3f,
                    1 + -Mathf.Abs(Mathf.Cos(-t * 3.14159f)), 0f);

                posOfs = pos + playerCamera.transform.localToWorldMatrix.MultiplyVector(
                    Vector3.SmoothDamp(Vector3.zero, 
                        bobbing * 0.13f * vLength / playerController.GetSpeedRatio(), 
                        ref dampCameraBobblingVelocity, 0.05f));
                
                zAngle = 0.1f * Mathf.Sin(-3.14159f * t) * Mathf.Min(1, vLength / 5);
                
                return;
            }
            posOfs = pos;
            zAngle = 0;
        }

        private void HandleFunctionalKey()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetPlayer();
            }
            
            if (Input.GetKeyDown(KeyCode.T))
            {
                RandomTeleportPlayer();
            }
        }

        public void HandleViewRotation(float mouseX, float mouseY)
        {
            rotAngle -= mouseY * mouseSpeed;
            rotAngle = Mathf.Clamp(rotAngle, -89, 89);

            horAngle += mouseX * mouseSpeed;

            transform.localEulerAngles = new Vector3(0, horAngle, 0);
            playerCamera.transform.localEulerAngles = new Vector3(rotAngle, horAngle, 0);
        }

        public KNBTTagCompound GetPlayerKNBTData()
        {
            var treeBase = new KNBTTagCompound("Player");
            var pos = transform.position;

            treeBase
                .SetFloat("pitch", rotAngle)
                .SetFloat("yaw", horAngle)
                .SetFloat("x", pos.x)
                .SetFloat("y", pos.y)
                .SetFloat("z", pos.z)
                .SetByte("flying", (byte) (playerController.flying ? 1 : 0));

            return treeBase;
        }

        public void SetPlayerWithKNBTData(KNBTTagCompound tree)
        {
            rotAngle = tree.GetFloat("pitch");
            horAngle = tree.GetFloat("yaw");

            var x = tree.GetFloat("x");
            var y = tree.GetFloat("y");
            var z = tree.GetFloat("z");
            
            transform.position = new Vector3(x, y, z);
            entityBase.MoveAABBToWorldPosition();
        }

        private void UpdateRaycast()
        {
            var playerPos = playerCamera.transform.position;
            var currentLevel = gameClient.GetCurrentLevel();
            if (currentLevel != null)
            {
                var hit = currentLevel.RaycastBlocks(playerPos, playerPos + playerCamera.transform.forward * 10);
                    raycastBlockHit = hit;

                if (hit != null)
                {
                    var blockCode = currentLevel.GetBlock(hit.blockX, hit.blockY, hit.blockZ).blockCode;
                    var meta = BlockMetaDatabase.GetBlockMetaByCode(blockCode);
                    if (meta != null)
                    {
                        var aabb = meta.behavior.GetAABB(0, 0, 0);
                        selectBox.UpdateDetectBox(aabb, hit.ToIntVector3().ToUnityVector3(),
                            currentLevel.GetExposedFace(hit.blockX, hit.blockY, hit.blockZ));
                    }
                    else
                    {
                        
                        Destroy(GameClientDebugger.Instance.CreateDebugObject(
                            new Vector3(hit.blockX, hit.blockY, hit.blockZ), Color.red), 1f);
                    }
                }
            }
        }
    }
}