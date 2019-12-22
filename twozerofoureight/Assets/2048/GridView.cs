using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Two048 {

    public enum Direction {
        LEFT = 0,
        RIGHT = 1,
        UP = 2,
        DOWN = 3
    }

    public class GridView : MonoBehaviour {
        public event Action<int, int> OnMerge;
        public event Action<int> OnScore;
        public event Action OnGameOver;

        private int ROW_COL_SIZE = 4;
        private int[, ] mGridPoints;
        private List<GridBox> mGridBoxList = new List<GridBox> ();
        private List<GridBox> mAnimatingBoxes = new List<GridBox> ();

        [SerializeField][Range (0.1f, 1)] private float m_AnimationDuration = 0.7f;
        [SerializeField] private float m_Margin = 10;
        [SerializeField] private float m_Padding = 5;
        [SerializeField] private GridBox m_GridUIPrefab;
        [SerializeField] private InputRecognition m_InputRecognition;

        private List<Vector2Int> mLastMergeBoxList = new List<Vector2Int> ();

        private List<Vector2Int> mEmptyIndexes;
        private bool mIsGameOver = false;
        private Ticker mAnimationDelayTicker = new Ticker ();

        void Start () {
            m_InputRecognition.OnSwipe += OnSwipeFromInput;
            mGridPoints = new int[ROW_COL_SIZE, ROW_COL_SIZE];
            mEmptyIndexes = new List<Vector2Int> ();

            MakeGrid ();
            SpawnRandom (1);
            SpawnRandom (1);

            PrintGrid ();
        }
        private void Update () {
            if (mAnimationDelayTicker.isRunning) {
                mAnimationDelayTicker.UpdateTick (Time.deltaTime);
                if (mAnimationDelayTicker.HasEnoughTimePassed ()) {
                    mAnimationDelayTicker.Execute ();
                }
            }
        }

        private void MakeGrid () {
            float totalSpan = transform.GetComponent<RectTransform> ().rect.width - (m_Margin * 2);
            float perBoxSize = totalSpan / ROW_COL_SIZE;
            float perBoxSizeBody = perBoxSize - (m_Padding * 2);

            float startIndexMultiplier = ROW_COL_SIZE % 2 == 0 ? ROW_COL_SIZE / 2 - 0.5f : ROW_COL_SIZE / 2;
            float startIndexX = perBoxSize * -startIndexMultiplier;
            float startIndexY = perBoxSize * startIndexMultiplier;

            float currentX = startIndexX;
            float currentY = startIndexY;

            for (int i = 0; i < ROW_COL_SIZE * ROW_COL_SIZE; i++) {
                GridBox gb = Instantiate (m_GridUIPrefab, transform);
                mGridBoxList.Add (gb);
                gb.SetBgSize (perBoxSizeBody);
                gb.transform.localPosition = new Vector2 (currentX, currentY);

                currentX += perBoxSize;
                if (i > 0 && ((i + 1) % ROW_COL_SIZE) == 0) {
                    currentY -= perBoxSize;
                    currentX = startIndexX;
                }

            }
        }

        private GridBox GetAnimatingBox () {
            foreach (GridBox item in mAnimatingBoxes) {
                if (!item.gameObject.activeInHierarchy)
                    return item;
            }

            GridBox gb = Instantiate (m_GridUIPrefab, transform);
            mAnimatingBoxes.Add (gb);
            return mAnimatingBoxes[mAnimatingBoxes.Count - 1];
        }

        private void AnimateBoxToBox (Vector2Int _fromIndex, Vector2Int _toIndex) {
            GridBox gbFrom = mGridBoxList[GetMapedInex (_fromIndex.x, _fromIndex.y)];
            GridBox gbTo = mGridBoxList[GetMapedInex (_toIndex.x, _toIndex.y)];

            //Debug.Log ("converting from " + gbFrom.GetCurrentValue () + " >> " + gbTo.GetCurrentValue ());
            Vector2 posFrom = gbFrom.transform.localPosition;
            Vector2 posTo = gbTo.transform.localPosition;

            GridBox dummyBox = GetAnimatingBox ();
            dummyBox.ModifyValue (gbFrom.GetCurrentValue ());
            dummyBox.SetBgSize (gbFrom.GetComponent<RectTransform> ().sizeDelta.x);
            dummyBox.transform.localPosition = posFrom;

            dummyBox.gameObject.SetActive (true);
            gbFrom.ModifyValue (0);

            dummyBox.transform.DOLocalMove (posTo, m_AnimationDuration).OnComplete (() => {
                dummyBox.gameObject.SetActive (false);
                PrintGrid ();
            });
        }

        private void MergeAt (Vector2Int _gridId, float _delay) {
            mLastMergeBoxList.Add (_gridId);
            mAnimationDelayTicker.SetFrequency (_delay);
        }

        private void QueMergeAnimation () {
            mAnimationDelayTicker.QueAction (() => {
                foreach (var item in mLastMergeBoxList) {
                    int gridIndex = GetMapedInex (item.x, item.y);
                    int value = GetGridValueByPoint (mGridPoints[item.x, item.y]);

                    //Debug.Log ("merged value " + value + " at " + item + " for " + gridIndex);
                    mGridBoxList[gridIndex].AnimateValue (value);

                    OnMerge?.Invoke (item.x, item.y);
                    OnScore?.Invoke (mGridPoints[item.x, item.y]);
                }

                mLastMergeBoxList.Clear ();
            });
            mAnimationDelayTicker.Start ();
        }

        private void OnDestroy () {
            OnMerge = null;
            OnGameOver = null;
            OnScore = null;
        }

        private void SpawnRandom (int _preferredValue = -1) {
            mEmptyIndexes.Clear ();
            for (int i = 0; i < ROW_COL_SIZE; i++) {
                for (int j = 0; j < ROW_COL_SIZE; j++) {
                    if (mGridPoints[i, j] == 0) {
                        mEmptyIndexes.Add (new Vector2Int (i, j));
                    }
                }
            }

            //Debug.Log ("total empty : " + mEmptyIndexes.Count);
            if (mEmptyIndexes.Count > 0) {
                Vector2Int randIndex = mEmptyIndexes[UnityEngine.Random.Range (0, mEmptyIndexes.Count)];
                //Debug.Log ("spawning at " + randIndex);
                int[] randPool = new int[] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3 };
                int value = _preferredValue > 0 ? _preferredValue : randPool[UnityEngine.Random.Range (0, randPool.Length)];

                mGridPoints[randIndex.x, randIndex.y] = value;
                mGridBoxList[GetMapedInex (randIndex.x, randIndex.y)].AnimateValue (GetGridValueByPoint (value));
            }
        }

        private void OnSwipeFromInput (Direction _dir) {
            //complete pending operations if any
            mLastMergeBoxList.Clear ();
            PrintGrid ();
            foreach (var item in mAnimatingBoxes) {
                item.gameObject.SetActive (false);
            }

            if (mIsGameOver) { return; }

            if (SwipeGrid (_dir)) {
                QueMergeAnimation ();
                Invoke ("SpawnNew", m_AnimationDuration);
            } else {
                Debug.Log ("No valid movement found");
                CheckGameOver ();
            }
        }

        private void SpawnNew () {
            //Debug.Log ("spawning new");
            SpawnRandom ();
            PrintGrid ();
            CheckGameOver ();
        }

        private void CheckGameOver () {
            //First check if there is no empty value
            for (int i = 0; i < ROW_COL_SIZE; i++) {
                for (int j = 0; j < ROW_COL_SIZE; j++) {
                    if (mGridPoints[i, j] == 0) {
                        mIsGameOver = false;
                        return;
                    }
                }
            }

            for (int i = 0; i < ROW_COL_SIZE; i++) {
                for (int j = 0; j < ROW_COL_SIZE - 1; j++) {
                    if (mGridPoints[i, j] == mGridPoints[i, j + 1]) {
                        Debug.Log ("found same value on row inspection");
                        mIsGameOver = false;
                        return;
                    }
                }
            }

            for (int i = 0; i < ROW_COL_SIZE; i++) {
                for (int j = 0; j < ROW_COL_SIZE - 1; j++) {
                    if (mGridPoints[j, i] == mGridPoints[j + 1, i]) {
                        Debug.Log ("found same value on column inspection");
                        mIsGameOver = false;
                        return;
                    }
                }
            }

            OnGameOverConfirm ();
        } //checkgameover

        private void OnGameOverConfirm () {
            mIsGameOver = true;
            OnGameOver?.Invoke ();
        }

        public bool SwipeGrid (Direction _dir) {
            //_dir%2 left/UP
            bool hasChanged = false;
            int startIndex = ((int) _dir % 2 == 0) ? 0 : ROW_COL_SIZE - 1;
            int endIndex = ((int) _dir % 2 == 0) ? ROW_COL_SIZE : -1;
            int modifier = ((int) _dir % 2 == 0) ? 1 : -1;

            if (_dir == Direction.LEFT || _dir == Direction.RIGHT) {
                for (int w = 0; w < ROW_COL_SIZE; w++) {
                    for (int i = startIndex; i != endIndex; i += modifier) {
                        int tValue = mGridPoints[w, i];
                        for (int j = i + modifier; j != endIndex; j += modifier) {
                            if (tValue != 0 && mGridPoints[w, j] == tValue) { //merge
                                mGridPoints[w, i] = mGridPoints[w, i] + 1;
                                mGridPoints[w, j] = 0;
                                tValue = mGridPoints[w, i];

                                AnimateBoxToBox (new Vector2Int (w, j), new Vector2Int (w, i));
                                MergeAt (new Vector2Int (w, i), m_AnimationDuration);
                                hasChanged = true;
                                //break;
                            } else if (tValue == 0 && mGridPoints[w, j] != 0) { //swap
                                mGridPoints[w, i] = mGridPoints[w, j];
                                mGridPoints[w, j] = 0;

                                tValue = mGridPoints[w, i];
                                AnimateBoxToBox (new Vector2Int (w, j), new Vector2Int (w, i));
                                hasChanged = true;
                                //break;
                            } else if (tValue != 0 && mGridPoints[w, j] != 0 && mGridPoints[w, j] != tValue) {
                                break; // 2 | 0 | 4 | 2, it will match first and last 2 together
                            }
                        }
                    }
                }
            } else if (_dir == Direction.UP || _dir == Direction.DOWN) {
                for (int w = 0; w < ROW_COL_SIZE; w++) {
                    for (int i = startIndex; i != endIndex; i += modifier) {
                        int tValue = mGridPoints[i, w];
                        for (int j = i + modifier; j != endIndex; j += modifier) {
                            if (tValue != 0 && mGridPoints[j, w] == tValue) { //merge
                                mGridPoints[i, w] = mGridPoints[i, w] + 1;
                                mGridPoints[j, w] = 0;
                                tValue = mGridPoints[i, w];

                                AnimateBoxToBox (new Vector2Int (j, w), new Vector2Int (i, w));
                                MergeAt (new Vector2Int (i, w), m_AnimationDuration);
                                hasChanged = true;
                                //break;
                            } else if (tValue == 0 && mGridPoints[j, w] != 0) { //swap
                                mGridPoints[i, w] = mGridPoints[j, w];
                                mGridPoints[j, w] = 0;

                                tValue = mGridPoints[i, w];
                                AnimateBoxToBox (new Vector2Int (j, w), new Vector2Int (i, w));
                                hasChanged = true;
                                //break;
                            } else if (tValue != 0 && mGridPoints[j, w] != 0 && mGridPoints[j, w] != tValue) {
                                break; // 2 | 0 | 4 | 2, it will match first and last 2 together
                            }
                        }
                    }
                }
            }

            return hasChanged;
        }

        void PrintGrid (bool animate = true) {
            for (int i = 0; i < ROW_COL_SIZE; i++) {
                for (int j = 0; j < ROW_COL_SIZE; j++) {
                    int mapIndex = GetMapedInex (i, j);
                    int value = GetGridValueByPoint (mGridPoints[i, j]);
                    mGridBoxList[mapIndex].ModifyValue (value);
                }
            }

        }

        int GetMapedInex (int row, int col) {
            return ((row + col) + ((ROW_COL_SIZE - 1) * row));
        }

        private int GetGridValueByPoint (int _pow) {
            int value = (int) Mathf.Pow (2, _pow);
            return value < 2 ? 0 : value;
        }
    }
}