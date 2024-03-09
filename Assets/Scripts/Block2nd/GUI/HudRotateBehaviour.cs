using Block2nd.GamePlay;
using UnityEngine;

namespace Block2nd.GUI
{
    public class HudRotateBehaviour : MonoBehaviour
    {
        private Player player;
        private float lastPlayerHorAngle;
        private float lastPlayerRotAngle;

        private float lastRotDelta;
        private float lastHorDelta;

        private void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        private void Start()
        {
            lastPlayerHorAngle = player.horAngle;
            lastPlayerRotAngle = player.rotAngle;
        }

        private void Update()
        {
            var horDelta = (player.horAngle - lastPlayerHorAngle + lastHorDelta) / 4f;
            var rotDelta = (player.rotAngle - lastPlayerRotAngle + lastRotDelta) / 4f;

            var targetEulerAngel = new Vector3(-rotDelta, -horDelta);
            transform.localEulerAngles = targetEulerAngel;

            lastPlayerHorAngle = player.horAngle;
            lastPlayerRotAngle = player.rotAngle;
            lastHorDelta = horDelta;
            lastRotDelta = rotDelta;
        }
    }
}