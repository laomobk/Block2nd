using System;
using System.Collections;
using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.GUI;
using Block2nd.Render;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.MathUtil;
using Block2nd.World;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Block2nd.GamePlay
{
    public class Player : MonoBehaviour
    {
        private System.Random random = new System.Random();
        
        private CharacterController controller;
        private GameClient gameClient;

        private RaycastHit raycastBlockHit;
        private Vector3 raycastHitPointBlockNormalAlong;
        private bool isRaycastHitBlock;

        private Inventory inventory = new Inventory();
        private int holdingBlockCode = 1;

        public float horAngle = 0f;
        public float rotAngle = 0.0f;
        private Vector3 prevPlayerPos = new Vector2(0, 1);

        private float mouseSpeed = 25f;

        private ChunkBlockData selectedBlock;
        private SelectBox selectBox;

        [HideInInspector] public PlayerController playerController;

        public Camera playerCamera;
        public LayerMask raycastLayerMask;

        public HoldingBlockPreview holdingBlockPreview;

        public Inventory Inventory => inventory;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
        }
        
        private void Start()
        {
            SetHoldingBlock(1);

            selectBox = GameObject.FindWithTag("SelectBox").GetComponent<SelectBox>();
            controller = GetComponent<CharacterController>();
            transform.position = new Vector3(15, 13, 3);

            UpdateSelectedBox();

            UpdateHoldingItemNameText();
        }

        void Update()
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 10);
            
            UpdateSelectedBox();
            HandleFunctionalKey();
            HandleItemKey();
            
            gameClient.guiCanvasManager.inventoryUI.RenderInventory(inventory);
            
            selectBox.gameObject.SetActive(isRaycastHitBlock);

            if (gameClient.GameClientState == GameClientState.GAME && !gameClient.gameSettings.mobileControl)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (isRaycastHitBlock)
                    {
                        DestroyBlock();
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (isRaycastHitBlock)
                    {
                        PlaceBlock();
                    }
                }

                var x = Input.GetAxis("Mouse X") * 0.1f;
                var y = Input.GetAxis("Mouse Y") * 0.1f;

                HandleViewRotation(x, y);
            }

            var camPos = transform.position + Vector3.up * 0.8f;

            playerCamera.transform.position = camPos;
        }

        public void ResetPlayer()
        {
            var levelWidth = gameClient.worldSettings.levelWidth;
            
            int x = levelWidth / 2;
            int z = levelWidth / 2;
            
            gameClient.CurrentLevel.ChunkManager.BakeAllChunkHeightMap();
            
            var chunk = gameClient.CurrentLevel.ChunkManager.FindChunk(x, z);
            var chunkLocalPos = chunk.WorldToLocal(x, z);
            
            var y = chunk.heightMap[chunkLocalPos.x, chunkLocalPos.z] + 5;
            
            ResetPlayer(new Vector3(x, y, z));
        }

        public void RandomTeleportPlayer()
        {
            var levelWidth = gameClient.worldSettings.levelWidth;

            var x = random.Next(levelWidth / 2 - levelWidth / 4, levelWidth / 2 + levelWidth / 4);
            var z = random.Next(levelWidth / 2 - levelWidth / 4, levelWidth / 2 + levelWidth / 4);
            
            gameClient.CurrentLevel.ChunkManager.BakeAllChunkHeightMap();
            
            var chunk = gameClient.CurrentLevel.ChunkManager.FindChunk(x, z);
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
                gameClient.guiCanvasManager.SetGUIItemNameText(
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

            holdingBlockPreview.SetMeshFromShape(meta.shape);
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

        public void PlaceBlock()
        {
            var blockCenter = raycastHitPointBlockNormalAlong + new Vector3(0.5f, 0.5f, 0.5f);
            if (GetComponent<BoxCollider>().bounds.Contains(blockCenter))
            {
                return;
            }
            
            var level = gameClient.GetCurrentLevel();
            
            var worldPos = level.CalculateWorldBlockPosByHit(raycastBlockHit);

            Chunk cp;
            var defaultAction = level.GetBlock(worldPos, out cp)
                                    .behaviorInstance
                                        .OnInteract(
                                            new IntVector3(worldPos), level, cp, this);

            if (!defaultAction)
                return;
            
            var intPos = new IntVector3(raycastHitPointBlockNormalAlong);
            
            BlockMetaDatabase.GetBlockMetaByCode(holdingBlockCode).behavior.OnPlace(
                ref intPos, level, cp, this);
            
            level.SetBlock(holdingBlockCode, intPos.x, intPos.y, intPos.z, true);
            holdingBlockPreview.PlayUseBlockAnimation();
        }

        public void DestroyBlock()
        {
            var level = gameClient.GetCurrentLevel();
            var worldPos = level.CalculateWorldBlockPosByHit(raycastBlockHit);
            level.CreateBlockParticle(worldPos);
            level.SetBlock(0, worldPos, true, triggerUpdate: true);
            holdingBlockPreview.PlayUseBlockAnimation();
        }

        private Vector3 ApplyViewBobbing(Vector3 pos)
        {
            var speedVector = transform.localToWorldMatrix.MultiplyVector(playerController.playerSpeed);
            speedVector.y = 0;
            Debug.Log(speedVector);
            
            var speed = speedVector.magnitude * Time.time * 0.1f;
            pos += transform.localToWorldMatrix.MultiplyVector(new Vector3(Mathf.Sin(-3.14159f * speed * 0.5f), 
                                -Mathf.Abs(Mathf.Cos(-speed * 3.14159f)), 
                                0f));

            return pos;
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
        
        private void UpdateSelectedBox()
        {
            RaycastHit hit;

            if (Physics.Raycast(playerCamera.transform.position,
                    playerCamera.transform.forward, out hit, 10,
                    layerMask: raycastLayerMask))
            {
                Debug.DrawRay(hit.point, hit.normal, Color.red);

                if (hit.collider.CompareTag("WorldDetectBox"))
                {
                    var level = gameClient.GetCurrentLevel();
                    var worldPos = level.CalculateWorldBlockPosByHit(hit);

                    var code = level.GetBlock((int) worldPos.x, (int) worldPos.y, (int) worldPos.z).blockCode;

                    var meta = BlockMetaDatabase.GetBlockMetaByCode(code);
                    if (meta != null)
                    {
                        selectBox.UpdateDetectBoxByShape(meta.shape, worldPos, level.GetExposedFace(
                            (int) worldPos.x, (int) worldPos.y, (int) worldPos.z));
                    }

                    isRaycastHitBlock = true;
                    raycastBlockHit = hit;
                    raycastHitPointBlockNormalAlong = worldPos + hit.normal;
                }
                else
                {
                    isRaycastHitBlock = false;
                }
            }
            else
            {
                isRaycastHitBlock = false;
            }
        }
    }
}