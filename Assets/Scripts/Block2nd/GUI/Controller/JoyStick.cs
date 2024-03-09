using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Block2nd.GUI
{
    public class JoyStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public float overflowStickLength = 100f;
        public float maxStickLength = 100f;
        public float minActiveStickLength = 10f;

        public Camera uiCamera;

        public UnityEvent onJoystickClick;
        
        private RectTransform centerTransform;
        private RectTransform stickRectTransform;

        private bool overflowStick;
        private bool canStickClick;

        private bool inTouch;
        private int pointerId;
        private float touchBeginTime;

        private Vector2 axis;

        public Vector2 Axis => axis;
        public bool OverflowStick => overflowStick;

        private void Awake()
        {
            centerTransform = transform.GetChild(1).GetComponent<RectTransform>();
            stickRectTransform = transform.GetChild(2).GetComponent<RectTransform>();
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (inTouch)
            {
                var touches = Input.touches;
                if (pointerId >= 0 && pointerId < touches.Length)
                {
                    var touch = touches[pointerId];
                    
                    OnDrag(touch.position);
                } 
            }
        }

        public void OnDrag(Vector2 mousePos)
        {
            var centerPos = centerTransform.anchoredPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(centerTransform, mousePos, 
                uiCamera, out Vector2 point);

            var dir = point - centerPos;
            var realDir = dir;

            if (dir.magnitude < minActiveStickLength)
            {
                if (canStickClick)
                {
                    onJoystickClick.Invoke();
                    canStickClick = false;
                }
                dir = Vector2.zero;
            }
            else
            {
                canStickClick = true;
            }

            if (dir.magnitude > maxStickLength)
            {
                dir = dir.normalized * maxStickLength;
            }

            axis = dir / maxStickLength;

            if (realDir.magnitude > overflowStickLength)
            {
                overflowStick = true;
            }
            else
            {
                overflowStick = false;
            }

            stickRectTransform.anchoredPosition = centerPos + dir;
        }

        public void OnEndDrag()
        {
            stickRectTransform.anchoredPosition = centerTransform.anchoredPosition;
            axis = Vector2.zero;
            canStickClick = false;
            overflowStick = false;
        }

        public void OnPointerDown(PointerEventData pointerEventData)
        {
            var mousePos = Input.mousePosition;
            var centerPos = centerTransform.anchoredPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(centerTransform, mousePos, 
                uiCamera, out Vector2 point);

            var dir = point - centerPos;

            if (dir.magnitude < minActiveStickLength)
            {
                onJoystickClick.Invoke();
            }

            pointerId = pointerEventData.pointerId;
            inTouch = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnEndDrag();
            inTouch = false;
        }
    }
}