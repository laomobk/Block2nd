using System;
using Block2nd.Client;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Block2nd.GUI
{
    public class GamePadButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent callback;
        public UnityEvent recoveryCallback;
        
        private GameClient gameClient;
        
        private bool active;

        private void Start()
        {
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
        }

        private void Update()
        {
            if (active)
            {
                callback.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            active = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            active = false;
            recoveryCallback.Invoke();
        }
    }
}