using System;
using Block2nd.Client;
using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.GUI
{
    public class HandShakingBehavior : MonoBehaviour
    {
        private PlayerController playerController;
        private GameClient gameClient;
        private float oldRotationY = 0;

        private void Awake()
        {
            playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
        }

        private void Update()
        {
            if (gameClient.GameClientState == GameClientState.GAME)
            {
                var euler = transform.localEulerAngles;

                if (!playerController.OnGround)
                {
                    euler.x = playerController.playerSpeed.y * 0.3f;
                }
                else
                {
                    var v = playerController.playerSpeed;
                    v.y = 0;

                    var shake = ShakeAnimation(Time.time * 2.5f + (1 + v.magnitude / 2.5f)) * 0.2f;

                    var pos = Vector3.Lerp(Vector3.zero, shake, v.magnitude / 8f);
                    var d = pos - transform.localPosition;
                    transform.localPosition = Vector3.Lerp(
                        transform.localPosition,
                        pos, 0.15f - Mathf.Max(d.magnitude / 50f, 0.05f));
                }
                
                transform.localEulerAngles = euler;
            }

            oldRotationY = playerController.transform.eulerAngles.y;
        }

        private Vector3 ShakeAnimation(float t)
        {
            var x = 0.5f * Mathf.Sin(-3.14159f * (t + 3.1415926f));
            var y = 0.7f + -Mathf.Abs(Mathf.Cos(-t * 3.14159f)) * 2f;

            return new Vector3(x, y, 0);
        }
    }
}