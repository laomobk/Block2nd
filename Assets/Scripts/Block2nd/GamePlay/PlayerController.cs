using System;
using System.Collections.Generic;
using Block2nd.Client;
using Block2nd.Database;
using Block2nd.Database.Meta;
using Block2nd.Entity;
using Block2nd.MathUtil;
using Block2nd.Resource;
using Block2nd.World;
using UnityEngine;
using UnityEngine.Profiling;

namespace Block2nd.GamePlay
{
    public class PlayerController : MonoBehaviour
    {
        private GameClient gameClient;

        private PlayerEntityBase entityBase;

        private Vector3 playerControlSpeed = Vector3.zero;
        [HideInInspector] public Vector3 playerSpeed;
        [HideInInspector] public Vector3 externalSpeed;

        public float runSpeedRatio = 10;
        public float walkSpeedRatio = 5;
        
        private float speedRatio = 1;
        private int waterCode;
        private bool floating = false;

        private bool floatBegin = true;
        private bool jumpBegin = false;

        private bool externalFloatKeyState = false;
        private float targetCameraFov;

        private float lastSpacePressTime = 0f;
        public bool flying = false;
        public float flySpeed;

        private bool inWater;
        public bool InWater => inWater;
        public bool OnGround => entityBase.OnGround;
        public PlayerEntityBase PlayerEntityBase => entityBase;
        public PlayerState playerState = PlayerState.WALK;
        public Camera playerCamera;
        
        private Queue<Vector3> impulseForceQueue = new Queue<Vector3>();

        private AudioSource audioSource;
        private float stepTimeClock = 0;
        
        private void Awake()
        {
            waterCode = BlockMetaDatabase.GetBlockCodeById("b2nd:block/water");
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
        }

        void Start()
        {
            targetCameraFov = gameClient.gameSettings.cameraFov;
            speedRatio = walkSpeedRatio;
            entityBase = GetComponent<PlayerEntityBase>();
            audioSource = GetComponent<AudioSource>();
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
            if (entityBase.OnGround)
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
            playerSpeed.y += 30f * Time.deltaTime;
        }

        public void MoveForward()
        {
            playerControlSpeed.z = 1f * speedRatio;
        }
        
        public void MoveBack()
        {
            playerControlSpeed.z = -1f * speedRatio;
        }
        
        public void MoveLeft()
        {
            playerControlSpeed.x = -1f * speedRatio;
        }
        
        public void MoveRight()
        {
            playerControlSpeed.x = 1 * speedRatio;
        }

        public void MoveAxis(Vector2 axis)
        {
            playerSpeed.x = axis.x * speedRatio * Time.deltaTime;
            playerSpeed.z = axis.y * speedRatio * Time.deltaTime;
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

                UpdateCameraFov();
            }
            else
            {
                speedRatio = walkSpeedRatio;
                playerState = PlayerState.WALK;
                
                UpdateCameraFov();
            }
        }

        private void UpdateCameraFov()
        {
            if (playerState == PlayerState.RUN)
            {
                targetCameraFov = gameClient.gameSettings.cameraFov + 5 + (flying ? 10 : 0);
            }
            else
            {
                targetCameraFov = gameClient.gameSettings.cameraFov + (flying ? 10 : 0);
            }
        }

        public float GetSpeedRatio()
        {
            return playerState == PlayerState.WALK ? walkSpeedRatio : runSpeedRatio;
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

        private void UpdateCameraLerp()
        {
            var fieldOfView = playerCamera.fieldOfView;
            fieldOfView = (fieldOfView + (targetCameraFov - fieldOfView) * 0.15f);
            playerCamera.fieldOfView = fieldOfView;
        }

        private void FixedUpdate()
        {
            UpdateCameraLerp();
        }

        public void PressSpace()
        {
            if (Time.time - lastSpacePressTime > 0.1f && Time.time - lastSpacePressTime < 0.35f)
            {
                playerSpeed.y = 0;
                flying = !flying;
                UpdateCameraFov();
            }

            lastSpacePressTime = Time.time;
        }

        void Update()
        {

            if (gameClient.GameClientState == GameClientState.GAME && gameClient.CurrentLevel != null)
            {
                floating = false;
                
                inWater = gameClient.CurrentLevel.GetBlock(
                    IntVector3.FromFloorVector3(transform.position)).blockCode == waterCode;
                
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    SetRunState(true);
                }
                else if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                    SetRunState(false);
                }
                
                if (!Application.isMobilePlatform && !gameClient.gameSettings.mobileControl)
                {
                    var hAxis = Input.GetAxis("Horizontal");
                    var vAxis = Input.GetAxis("Vertical");

                    playerControlSpeed.x = hAxis * speedRatio;
                    playerControlSpeed.z = vAxis * speedRatio;

                    if (playerControlSpeed.magnitude > speedRatio)
                    {
                        playerControlSpeed.Normalize();
                        playerControlSpeed *= speedRatio;
                    }

                    playerSpeed.x = playerControlSpeed.x;
                    playerSpeed.z = playerControlSpeed.z;
                }

                ApplyAcculation(ref externalSpeed);

                var onGround = entityBase.OnGround && !inWater;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    PressSpace();
                }

                if (!flying)
                {
                    if (inWater)
                    {
                        if (Input.GetKey(KeyCode.Space) || externalFloatKeyState)
                        {
                            Float();
                        }
                        else
                        {
                            floatBegin = true;
                            playerSpeed.y = -3f * Time.deltaTime;
                        }
                    }
                    else if (entityBase.OnGround && Input.GetKey(KeyCode.Space) && !jumpBegin)
                    {
                        Jump();
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        flySpeed = -5f;
                    } else if (Input.GetKey(KeyCode.Space) || externalFloatKeyState)
                    {
                        flySpeed = 5;
                    }
                    else if (!gameClient.gameSettings.mobileControl)
                    {
                        flySpeed = 0;
                    }

                    playerSpeed.y = flySpeed;
                }

                playerSpeed.x *= onGround ? 1 : 0.8f;
                playerSpeed.z *= onGround ? 1 : 0.8f;

                if (!flying)
                {
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
                }

                if (flying)
                    playerSpeed *= 1.5f; 

                var speed = transform.localToWorldMatrix.MultiplyVector(playerSpeed) + externalSpeed;

                if (entityBase.OnGround)
                    externalSpeed *= 0.75f;
                else
                    externalSpeed *= 0.9f;

                entityBase.forward = transform.forward;
                entityBase.MoveWorld(speed * Time.deltaTime);
                
                var horizontalSpeed = speed;
                horizontalSpeed.y = 0;
                
                var belowCode = gameClient.CurrentLevel.GetBlock(transform.position + Vector3.down * 1.2f).blockCode;
                stepTimeClock += horizontalSpeed.magnitude * Time.deltaTime;

                if (belowCode != 0 && 
                    gameClient.CurrentLevel != null && onGround && horizontalSpeed.magnitude * Time.deltaTime > 0)
                {
                    if (stepTimeClock > 2f)
                    {
                        stepTimeClock = 0;
                        var behavior = BlockMetaDatabase.GetBlockBehaviorByCode(belowCode);
                        if (behavior != null)
                        {
                            var clip = ResourceManager.Load<AudioClip>(
                                behavior.SoundDescriptor.stepSoundGroup.GetPath());
                            audioSource.clip = clip;
                            audioSource.Play();
                        }
                    }
                }

                if (entityBase.HitFront)
                {
                    Jump();
                }

                if (entityBase.HitTop)
                {
                    playerSpeed.y = 0;
                }
            }
        }
    }
}