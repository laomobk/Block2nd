using System;
using Block2nd.Client;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Block2nd.GUI
{
    public class InteractivePanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent longPressCallback;
        public UnityEvent tapCallback;

        public float pressTriggerInterval = 0.5f;

        private float pressTriggerTime;
        private float pressBeginTime;

        private bool active = false;

        private GameClient gameClient;

        private void Awake()
        {
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
            pressTriggerTime = -pressBeginTime;
        }

        private void Update()
        {
            if (gameClient.GameClientState == GameClientState.GAME)
            {
                if (active)
                {
                    if (Time.time - pressBeginTime > 1)
                    {
                        if (Time.time - pressTriggerTime >= pressTriggerInterval)
                        {
                            longPressCallback.Invoke();
                            pressTriggerTime = Time.time;
                        }
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            active = true;
            
            pressBeginTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Time.time - pressBeginTime < 0.1f)
            {
                tapCallback.Invoke();
            }

            active = false;
        }
    }
}