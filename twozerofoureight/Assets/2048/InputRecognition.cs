using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Two048 {
    public class InputRecognition : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
        public event Action<Direction> OnSwipe;
        private Vector2 mStartPos;
        private Vector2 mEndPos;

        public void OnBeginDrag (PointerEventData eventData) {
            mStartPos = mMainCam.ScreenToWorldPoint (eventData.position);
        }

        private void OnDestroy () {
            OnSwipe = null;
        }

        public void OnDrag (PointerEventData eventData) { }

        public void OnEndDrag (PointerEventData eventData) {
            mEndPos = mMainCam.ScreenToWorldPoint (eventData.position);
            Vector2 diff = mEndPos - mStartPos;
            if (diff.sqrMagnitude < 0.1f) {
                Debug.Log ("swipe strength too small to recognize");
            } else {
                diff = diff.normalized;
                float angleInDegree = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg;
                angleInDegree = angleInDegree < 0 ? 360 + angleInDegree : angleInDegree;

                Direction swipeDir = Direction.UP;
                if (angleInDegree >= 40 && angleInDegree < 140) {
                    swipeDir = Direction.UP;
                } else if (angleInDegree >= 140 && angleInDegree < 230) {
                    swipeDir = Direction.LEFT;
                } else if (angleInDegree >= 230 && angleInDegree < 320) {
                    swipeDir = Direction.DOWN;
                } else if (angleInDegree >= 320 || (angleInDegree >= 0 && angleInDegree < 40)) {
                    swipeDir = Direction.RIGHT;
                }

                //Debug.Log ("swipe dir :: " + swipeDir + " , " + angleInDegree);
                OnSwipe?.Invoke (swipeDir);
            }
        }

        private Camera mMainCam;
        void Start () {
            mMainCam = Camera.main;
        }

        //input ssytem for editor
        void Update () {
            if (Input.GetKeyDown (KeyCode.RightArrow)) {
                OnSwipe (Direction.RIGHT);
            } else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
                OnSwipe (Direction.LEFT);
            } else if (Input.GetKeyDown (KeyCode.UpArrow)) {
                OnSwipe (Direction.UP);
            } else if (Input.GetKeyDown (KeyCode.DownArrow)) {
                OnSwipe (Direction.DOWN);
            }
        }
    }
}