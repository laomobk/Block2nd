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
        private bool moveTooFar = false;
        private bool continuous = false;
        private Vector2 touchBeginPosition = Vector2.zero;

        private int pointerId;
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
                    Debug.Log(moveTooFar);

                    var touches = Input.touches;
                    if (pointerId >= 0 && pointerId < touches.Length)
                    {
                        var touch = touches[pointerId];
                        if ((touch.position - touchBeginPosition).magnitude > 12f)
                        {
                            moveTooFar = true;
                        }
                    }

                    if (continuous || !moveTooFar)
                    {
                        if (Time.time - pressBeginTime > 0.15f)
                        {
                            if (Time.time - pressTriggerTime >= pressTriggerInterval)
                            {
                                longPressCallback.Invoke();
                                pressTriggerTime = Time.time;
                                continuous = true;
                            }
                        }
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerId = eventData.pointerId;
            pressBeginTime = Time.time;
            moveTooFar = false;
            active = true;
            continuous = false;
            touchBeginPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Time.time - pressBeginTime < 0.1f)
            {
                tapCallback.Invoke();
            }

            active = false;
            moveTooFar = false;
            continuous = false;
        }
    }
}