using System;
using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.Entity;
using Block2nd.MathUtil;
using Block2nd.World;
using UnityEngine;
using UnityEngine.Profiling;

namespace Block2nd.GamePlay
{
    public class PlayerController : MonoBehaviour
    {
        private GameClient gameClient;

        private PlayerEntity entity;
        
        [HideInInspector] public Vector3 playerSpeed;
        public Vector3 externalSpeed;

        public float runSpeedRatio = 10;
        public float walkSpeedRatio = 5;
        
        private float speedRatio = 1;
        private int waterCode;
        private bool floating = false;

        private bool floatBegin = true;
        private bool jumpBegin = false;

        private bool externalFloatKeyState = false;

        private bool inWater;
        public bool InWater => inWater;

        public PlayerState playerState = PlayerState.WALK;

        public Camera playerCamera;

        private Queue<Vector3> impulseForceQueue = new Queue<Vector3>();
        
        private void Awake()
        {
            waterCode = BlockMetaDatabase.GetBlockCodeById("b2nd:block/water");
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
        }

        void Start()
        {
            speedRatio = walkSpeedRatio;
            entity = GetComponent<PlayerEntity>();
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

        public void ResetVelocity()
        {
            playerSpeed = Vector3.zero;
        }

        public void SetFloatKeyState(bool state)
        {
            externalFloatKeyState = state;
        }

        public void Jump()
        {
            if (entity.OnGround)
            {
                playerSpeed.y = 9f;
                jumpBegin = true;
            }
        }

        public void Float()
        {
            if (floatBegin)
            {
                playerSpeed.y = 0f;
            }

            floatBegin = false;
            floating = true;
            playerSpeed.y += 17f * Time.deltaTime;
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
            if (gameClient.GameClientState == GameClientState.GAME)
            {
                floating = false;
                
                inWater = gameClient.CurrentLevel.GetBlock(transform.position).blockCode == waterCode;

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
                
                ApplyAcculation(ref externalSpeed);

                var onGround = entity.OnGround && !inWater;

                if (inWater)
                {
                    if (Input.GetKey(KeyCode.Space) || externalFloatKeyState)
                    {
                        Float();
                    } else {
                        floatBegin = true;
                        playerSpeed.y = -3f;
                    }
                } else if (entity.OnGround && Input.GetKey(KeyCode.Space) && !jumpBegin)
                {
                    Jump();
                }
                playerSpeed.x *= onGround ? 1 : 0.8f;
                playerSpeed.z *= onGround ? 1 : 0.8f;

                if (!onGround)
                {
                    if (!inWater)
                    {
                        playerSpeed.y -= 30f * Time.deltaTime;
                    }
                }
                else
                {
                    if (!jumpBegin)
                    {
                        playerSpeed.y = -1f;
                    }
                    else
                    {
                        jumpBegin = false;
                    }
                }

                var speed = transform.localToWorldMatrix.MultiplyVector(playerSpeed) + externalSpeed;

                if (entity.OnGround)
                    externalSpeed *= 0.75f;
                else
                    externalSpeed *= 0.9f;

                entity.MoveWorld(speed * Time.deltaTime);
            }
        }
    }
}