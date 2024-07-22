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
        public Camera uiCamera;
        public GameObject touchCircle;
        
        public float pressTriggerInterval = 0.5f;

        private float pressTriggerTime;
        private float pressBeginTime;

        private bool active = false;
        private bool moveTooFar = false;
        private bool continuous = false;
        private Vector2 touchBeginPosition = Vector2.zero;

        private int pointerId;
        private GameClient gameClient;

        private RectTransform rectTransform;
        private RectTransform circleRectTransform;

        private void Awake()
        {
            gameClient = GameObject.FindWithTag("GameClient").GetComponent<GameClient>();
            pressTriggerTime = -pressBeginTime;
            circleRectTransform = touchCircle.GetComponent<RectTransform>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (gameClient.GameClientState == GameClientState.GAME)
            {
                if (active)
                {
                    
                    var touch = Input.GetTouch(pointerId);
                    if ((touch.position - touchBeginPosition).magnitude > 12f)
                    {
                        moveTooFar = true;
                    }

                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        rectTransform, touch.position, uiCamera, out var vec2d);
                    circleRectTransform.anchoredPosition = vec2d;

                    if (continuous || !moveTooFar)
                    {
                        if (Time.time - pressBeginTime > 0.15f)
                        {
                            if (Time.time - pressTriggerTime >= pressTriggerInterval)
                            {
                                OnPanelLongPress(touch.position);
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
            touchCircle.SetActive(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Time.time - pressBeginTime < 0.1f)
            {
                OnPanelClick(eventData.position);
            }

            active = false;
            moveTooFar = false;
            continuous = false;
            touchCircle.SetActive(false);
        }
        
        public void OnPanelClick(Vector2 screenPos)
        {
            var level = gameClient.CurrentLevel;
            var player = gameClient.Player;

            var ray = player.playerCamera.ScreenPointToRay(screenPos);

            if (level == null || player == null)
                return;

            var hit = level.RaycastBlocks(ray.origin, ray.origin + ray.direction * 10);
            player.PlaceBlock(hit);
        }

        public void OnPanelLongPress(Vector2 screenPos)
        {
            var level = gameClient.CurrentLevel;
            var player = gameClient.Player;

            var ray = player.playerCamera.ScreenPointToRay(screenPos);

            if (level == null || player == null)
                return;

            var hit = level.RaycastBlocks(ray.origin, ray.origin + ray.direction * 10);
            player.DestroyBlock(hit);
        }
    }
}