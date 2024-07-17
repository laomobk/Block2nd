using System;
using Block2nd.Client;
using UnityEngine;

namespace Block2nd.Render
{
    public class CelestiaController : MonoBehaviour
    {
        private void RotateByTime(int levelTime)
        {
            levelTime %= 14400;

            var xAngel = -(levelTime / 14400f * 360f);
            transform.localEulerAngles = new Vector3(xAngel, 90, 0);
        }

        private void Update()
        {
            var level = GameClient.Instance.CurrentLevel;

            if (level != null)
            {
                RotateByTime(level.levelTime);
            }
        }
    }
}