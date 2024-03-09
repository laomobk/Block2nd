using System;
using Block2nd.Client;
using Block2nd.GamePlay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Block2nd.GUI.Controller
{
    public class InteractiveDragView : MonoBehaviour, IDragHandler
    {
        private GameClient gameClient;
        
        public Player player;
        [Range(1, 100)] public float sensitivity;

        private void Awake()
        {
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (gameClient.GameClientState == GameClientState.GAME)
            {
                var delta = eventData.delta;

                delta.x /= Screen.width;
                delta.y /= Screen.height;

                delta *= sensitivity;

                player.HandleViewRotation(delta.x, delta.y);
            }
        }
    }
}