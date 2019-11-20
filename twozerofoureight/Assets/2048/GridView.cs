using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Two048 {

    public enum Direction {
        LEFT = 0,
        RIGHT,
        UP,
        DOWN
    }

    public class GridView : MonoBehaviour {
        private int ROW_COL_SIZE = 4;
        private int[, ] mGridPoints;
        private List<GridBox> mGridBoxList = new List<GridBox> ();
        [SerializeField] private GridBox m_GridUIPrefab;
        [SerializeField] private InputRecognition m_InputRecognition;

        private List<Vector2Int> mEmptyIndexes;
        private bool mIsGameOver = false;

        void Start () {
            mGridPoints = new int[ROW_COL_SIZE, ROW_COL_SIZE];
            mEmptyIndexes = new List<Vector2Int> ();

            m_InputRecognition.OnSwipe += OnSwipeFromInput;

            for (int i = 0; i < ROW_COL_SIZE * ROW_COL_SIZE; i++) {
                GridBox gb = Instantiate (m_GridUIPrefab, transform);
                mGridBoxList.Add (gb);
            }

            mGridPoints[1, 1] = 1;
            mGridPoints[1, 3] = 1;
            mGridPoints[2, 3] = 1;

            PrintGrid ();
        }

        private void SpawnRandom () {
            mEmptyIndexes.Clear ();
            for (int i = 0; i < ROW_COL_SIZE; i++) {
                for (int j = i; j < ROW_COL_SIZE; j++) {
                    if (mGridPoints[i, j] == 0) {
                        mEmptyIndexes.Add (new Vector2Int (i, j));
                    }
                }
            }

            Vector2Int randIndex = mEmptyIndexes[UnityEngine.Random.Range (0, mEmptyIndexes.Count)];
            mGridPoints[randIndex.x, randIndex.y] = 1;
        }

        private void OnSwipeFromInput (Direction _dir) {
            SwipeGrid (_dir);
            MergeSameValues (_dir);
            SwipeGrid (_dir); // swipe again to vanish 0 places
            SpawnRandom ();
            mIsGameOver = CheckGameOver ();
            PrintGrid ();
        }

        private bool CheckGameOver () {
            //First check if there is no empty value
            if (mEmptyIndexes.Count != 0) {
                return false;
            }

            //check if there is common value side by side
            // ASSUMING ROW_SIZE,COL_SIZE is equal
            for (int i = 0; i < ROW_COL_SIZE; i++) {
                for (int j = i; j < ROW_COL_SIZE - 1; j++) {
                    if (mGridPoints[i, j] == mGridPoints[i, j + 1]) {
                        return false;
                    }

                    if (mGridPoints[j, i] == mGridPoints[j + 1, i]) {
                        return false;
                    }
                }
            }

            return true;
        } //checkgameover

        public void SwipeGrid (Direction _dir) {
            if (_dir == Direction.LEFT || _dir == Direction.RIGHT) {
                for (int w = 0; w < ROW_COL_SIZE; w++) {
                    int startIndex = _dir == Direction.LEFT ? 0 : ROW_COL_SIZE - 1;
                    int endIndex = _dir == Direction.LEFT ? ROW_COL_SIZE : -1;
                    int modifier = _dir == Direction.LEFT ? 1 : -1;

                    for (int i = startIndex; i != endIndex; i += modifier) {
                        if (mGridPoints[w, i] == 0) {
                            for (int j = i + modifier; j != endIndex; j += modifier) {
                                if (mGridPoints[w, j] != 0) {
                                    mGridPoints[w, i] = mGridPoints[w, j];
                                    mGridPoints[w, j] = 0;
                                    break;
                                }
                            }
                        }
                    }
                }
            } else if (_dir == Direction.UP || _dir == Direction.DOWN) {
                for (int w = 0; w < ROW_COL_SIZE; w++) {
                    int startIndex = _dir == Direction.UP ? 0 : ROW_COL_SIZE - 1;
                    int endIndex = _dir == Direction.UP ? ROW_COL_SIZE : -1;
                    int modifier = _dir == Direction.UP ? 1 : -1;

                    for (int i = startIndex; i != endIndex; i += modifier) {
                        if (mGridPoints[i, w] == 0) {
                            for (int j = i + modifier; j != endIndex; j += modifier) {
                                if (mGridPoints[j, w] != 0) {
                                    mGridPoints[i, w] = mGridPoints[j, w];
                                    mGridPoints[j, w] = 0;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //            PrintGrid ();
        }

        private void MergeSameValues (Direction _dir) {
            if (_dir == Direction.LEFT || _dir == Direction.RIGHT) {
                for (int w = 0; w < ROW_COL_SIZE; w++) {
                    int startIndex = _dir == Direction.LEFT ? 0 : ROW_COL_SIZE - 1;
                    int endIndex = _dir == Direction.LEFT ? ROW_COL_SIZE : -1;
                    int modifier = _dir == Direction.LEFT ? 1 : -1;

                    int rowIndex = w;
                    for (int i = startIndex; i != endIndex; i += modifier) {
                        if (mGridPoints[rowIndex, i] == 0) continue;
                        for (int j = i + modifier; j != endIndex; j += modifier) {
                            if (mGridPoints[rowIndex, i] == mGridPoints[rowIndex, j]) {
                                mGridPoints[rowIndex, i]++;
                                mGridPoints[rowIndex, j] = 0;
                            }
                        }
                    }
                }
            } else if (_dir == Direction.UP || _dir == Direction.DOWN) {
                for (int w = 0; w < ROW_COL_SIZE; w++) {
                    int startIndex = _dir == Direction.UP ? 0 : ROW_COL_SIZE - 1;
                    int endIndex = _dir == Direction.UP ? ROW_COL_SIZE : -1;
                    int modifier = _dir == Direction.UP ? 1 : -1;

                    int colIndex = w;
                    for (int i = startIndex; i != endIndex; i += modifier) {
                        if (mGridPoints[i, colIndex] == 0) continue;
                        for (int j = i + modifier; j != endIndex; j += modifier) {
                            if (mGridPoints[i, colIndex] == mGridPoints[j, colIndex]) {
                                mGridPoints[i, colIndex]++;
                                mGridPoints[j, colIndex] = 0;
                            }
                        }
                    }
                }
            }
        }

        void PrintGrid (bool _printUI = true) {
            StringBuilder sb = new StringBuilder ();
            for (int i = 0; i < ROW_COL_SIZE; i++) {
                //sb.Append ("row :: " + i + ">> ");
                for (int j = 0; j < ROW_COL_SIZE; j++) {
                    sb.Append (mGridPoints[i, j] + " ");
                    if (!_printUI) continue;

                    int mapIndex = ((i + j) + ((ROW_COL_SIZE - 1) * i));
                    int value = GetGridValueByPoint (mGridPoints[i, j]);
                    mGridBoxList[mapIndex].ModifyText (value);
                }
                sb.Append ("\n");
            }
            Debug.Log (sb);
        }

        private int GetGridValueByPoint (int _pow) {
            int value = (int) Mathf.Pow (2, _pow);
            return value < 2 ? 0 : value;
        }

        //input ssytem for editor
        void Update () {
            if (Input.GetKeyDown (KeyCode.RightArrow)) {
                OnSwipeFromInput (Direction.RIGHT);
            } else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
                OnSwipeFromInput (Direction.LEFT);
            } else if (Input.GetKeyDown (KeyCode.UpArrow)) {
                OnSwipeFromInput (Direction.UP);
            } else if (Input.GetKeyDown (KeyCode.DownArrow)) {
                OnSwipeFromInput (Direction.DOWN);
            }
        }
    }
}