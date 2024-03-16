using System;
using Block2nd.Client;
using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.GUI
{
    public class HandShakingBehavior : MonoBehaviour
    {
        private PlayerController playerController;
        private Player player;
        private GameClient gameClient;
        private float oldRotationY = 0;
        private float oldRotationX = 0;

        private void Awake()
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
        }

        private void FixedUpdate()
        {
            if (gameClient.GameClientState == GameClientState.GAME)
            {
                var euler = transform.localEulerAngles;
                var eulerX = 0f;

                if (!playerController.OnGround)
                {
                    eulerX = playerController.playerSpeed.y * 0.3f;
                    var absX = Mathf.Abs(eulerX);
                    if (absX > 30)
                    {
                        absX = 30;
                    }

                    eulerX = Mathf.Sign(eulerX) * absX;
                }
                else
                {
                    var v = playerController.playerSpeed;
                    v.y = 0;
                    var vLength = Mathf.Min(playerController.GetSpeedRatio(), v.magnitude);

                    var shake = ShakeAnimation(Time.time * 2.5f + (1 + vLength / 2.5f)) * 0.2f;

                    var pos = Vector3.Lerp(Vector3.zero, shake, vLength / 8f);
                    var d = pos - transform.localPosition;
                    transform.localPosition = Vector3.Lerp(
                        transform.localPosition,
                        pos, 0.15f - Mathf.Max(d.magnitude / 50f, 0.05f));
                }

                euler.y = Mathf.LerpAngle(
                    euler.y,
                    oldRotationY - playerController.transform.eulerAngles.y,
                    0.1f);
                euler.x = Mathf.LerpAngle(
                        euler.x,
                        oldRotationX - player.playerCamera.transform.eulerAngles.x,
                        0.1f);
                euler.x = Mathf.LerpAngle(euler.x, eulerX, playerController.OnGround ? 0.1f : 0.8f);
                
                transform.localEulerAngles = euler;
            }

            oldRotationY = playerController.transform.eulerAngles.y;
            oldRotationX = player.playerCamera.transform.eulerAngles.x;
        }

        private Vector3 ShakeAnimation(float t)
        {
            var x = 0.5f * Mathf.Sin(-3.14159f * (t + 3.1415926f));
            var y = 2f + -Mathf.Abs(Mathf.Cos(-t * 3.14159f)) * 4f;

            return new Vector3(x, y, 0);
        }
    }
}