using System;
using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.MathUtil;
using Block2nd.World;
using UnityEngine;
using UnityEngine.Profiling;

namespace Block2nd.GamePlay
{
    public class PlayerController : MonoBehaviour
    {
        private BoxCollider playerCollider;
        private CharacterController controller;
        private GameClient gameClient;
        private bool touchedGround = false;

        private Bounds aabb = new Bounds(Vector3.zero, Vector3.zero);
        
        public Vector3 playerSpeed;
        public Vector3 externalSpeed;
        
        public float playerStep = 0.5f;
        public float gravity = 15f;
        
        public float runSpeedRatio = 10;
        public float walkSpeedRatio = 5;
        
        private float speedRatio = 5;

        public PlayerState playerState = PlayerState.WALK;

        public Camera playerCamera;

        private Queue<Vector3> impulseForceQueue = new Queue<Vector3>();

        public BlockMeta stepBlockMeta;

        private void Awake()
        {
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
        }

        void Start()
        {
            speedRatio = walkSpeedRatio;
            controller = GetComponent<CharacterController>();
        }

        private void UpdateGravity()
        {
            if ((SharedData.CollisionFlags & CollisionFlags.CollidedBelow) == 0)
            {
                playerSpeed.y -= gravity * Time.deltaTime;
            }
        }

        private void ApplyAcculation(ref Vector3 v)
        {
            var queueLength = impulseForceQueue.Count;

            for (int i = 0; i < queueLength; ++i)
            {
                var f = impulseForceQueue.Dequeue();
                v += f;
            }
        }

        private void UpdateGroundBlock()
        {
            var bottom = transform.position - transform.up;
            
            var fPointNW = bottom + Vector3.forward + Vector3.left;
            var fPointNE = bottom + Vector3.forward + Vector3.right;
            var fPointSW = bottom + Vector3.back + Vector3.left;
            var fPointSE = bottom + Vector3.back + Vector3.right;
            
            IntVector3 ipoint;
            ChunkBlockData block;

            var level = gameClient.CurrentLevel;

            if ((block = level.GetBlock(fPointNE, out Chunk chunk)).blockCode != 0)
                ipoint = new IntVector3(fPointNE);
            else if ((block = chunk.GetBlockWS(fPointNW)).blockCode != 0) 
                ipoint = new IntVector3(fPointNW);
            else if ((block = chunk.GetBlockWS(fPointSE)).blockCode != 0) 
                ipoint = new IntVector3(fPointSE);
            else if ((block = chunk.GetBlockWS(fPointSW)).blockCode != 0) 
                ipoint = new IntVector3(fPointSW);
            else
            {
                stepBlockMeta = null;
                return;
            }
            
            var meta = BlockMetaDatabase.GetBlockMetaByCode(block.blockCode);

            if (meta == null)
            {
                stepBlockMeta = null;
                return;
            }

            var blockAabb = new Bounds(meta.aabb.center + ipoint.ToUnityVector3(), meta.aabb.size);

            if (blockAabb.Intersects(aabb))
            {
                stepBlockMeta = meta;
            }
            else
            {
                stepBlockMeta = BlockMetaDatabase.blocks[0];
            }
        }

        public void ResetVelocity()
        {
            try
            {
                controller.attachedRigidbody.velocity = Vector3.zero;
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        public void Jump()
        {
            playerSpeed.y = 8f;
        }

        public void MoveForward()
        {
            playerSpeed.z = 1f * speedRatio;
        }
        
        public void MoveBack()
        {
            playerSpeed.z = -1f * speedRatio;
        }
        
        public void MoveLeft()
        {
            playerSpeed.x = -1f * speedRatio;
        }
        
        public void MoveRight()
        {
            playerSpeed.x = 1 * speedRatio;
        }

        public void MoveAxis(Vector2 axis)
        {
            playerSpeed.x = axis.x * speedRatio;
            playerSpeed.z = axis.y * speedRatio;
        }

        public void ResetXZVelocity()
        {
            playerSpeed.x = 0;
            playerSpeed.z = 0;
        }

        public void SetRunState(bool state)
        {
            if (state)
            {
                speedRatio = runSpeedRatio;
                playerState = PlayerState.RUN;

                playerCamera.fieldOfView = gameClient.gameSettings.cameraFov + 5;
            }
            else
            {
                speedRatio = walkSpeedRatio;
                playerState = PlayerState.WALK;
                
                playerCamera.fieldOfView = gameClient.gameSettings.cameraFov;
            }
        }

        public void JumpWithCheck()
        {
            var canJump = Physics.Raycast(transform.position, -transform.up, 1f);
            
            if (gameClient.gameSettings.infiniteJump || canJump)
            {
                Jump();
            }
        }

        public void AddImpulseForse(Vector3 force)
        {
            impulseForceQueue.Enqueue(force);
        }

        void Update()
        {
            aabb.center = transform.position;
            
            if (gameClient.GameClientState == GameClientState.GAME)
            {
                UpdateGravity();

                if (!Application.isMobilePlatform && !gameClient.gameSettings.mobileControl)
                {
                    var hAxis = Input.GetAxis("Horizontal");
                    var vAxis = Input.GetAxis("Vertical");

                    playerSpeed.x = hAxis * speedRatio;
                    playerSpeed.z = vAxis * speedRatio;
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    SetRunState(true);
                }
                else if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                    SetRunState(false);
                }

                if ((SharedData.CollisionFlags & CollisionFlags.CollidedBelow) != 0)
                {
                    if (Input.GetKey(KeyCode.W))
                    {
                        MoveForward();
                    }

                    if (Input.GetKey(KeyCode.A))
                    {
                        MoveLeft();
                    }

                    if (Input.GetKey(KeyCode.S))
                    {
                        MoveBack();
                    }

                    if (Input.GetKey(KeyCode.D))
                    {
                        MoveRight();
                    }
                }
                
                ApplyAcculation(ref externalSpeed);

                var speed = playerSpeed + transform.worldToLocalMatrix.MultiplyVector(externalSpeed);

                CollisionFlags flags = controller.Move(transform.TransformVector(speed * Time.deltaTime));
                if ((flags & CollisionFlags.Below) != 0)
                {
                    if (!touchedGround)
                    {
                        playerSpeed.y = 0;
                        touchedGround = true;
                    }
                }
                else
                {
                    touchedGround = false;
                }

                var down = -transform.up;
                var pos = transform.position;
                var canJump = Physics.Raycast(pos, down, 1f) ||
                              Physics.Raycast(pos + Vector3.forward * 0.5f, down, 1f) ||
                              Physics.Raycast(pos + Vector3.back * 0.5f, down, 1f) ||
                              Physics.Raycast(pos + Vector3.left * 0.5f, down, 1f) ||
                              Physics.Raycast(pos + Vector3.right * 0.5f, down, 1f);

                if (canJump || touchedGround)
                {
                    externalSpeed /= 1 + Time.deltaTime * 20f;
                }
                else
                {
                    externalSpeed /= 1 + Time.deltaTime * 10f;
                }
                
                if (externalSpeed.magnitude < 0.05f)
                    externalSpeed = Vector3.zero;

                if (Input.GetKeyDown(KeyCode.Space) && (gameClient.gameSettings.infiniteJump || canJump))
                {
                    Jump();
                }

                if (canJump)
                {
                    if ((flags & CollisionFlags.CollidedSides) != 0)
                    {
                        Jump();
                    }
                }

                SharedData.CollisionFlags = flags;
            }
        }
    }
}