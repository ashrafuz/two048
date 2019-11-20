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

        private int ROW_SIZE = 4;
        private int COL_SIZE = 4;
        private int[, ] mGridPoints;
        private List<GridBox> mGridBoxList = new List<GridBox> ();

        [SerializeField] private GridBox m_GridUIPrefab;

        void Start () {
            mGridPoints = new int[ROW_SIZE, COL_SIZE];

            for (int i = 0; i < ROW_SIZE * COL_SIZE; i++) {
                GridBox gb = Instantiate (m_GridUIPrefab, transform);
                mGridBoxList.Add (gb);
            }

            mGridPoints[1, 1] = 1;
            mGridPoints[1, 3] = 1;
            mGridPoints[2, 3] = 1;

            PrintGrid ();
        }

        private void ShiftGridByRow (Direction _dir, int _rowNum, int colStart, int _count) {
            Debug.Log ("sfhigting  " + _count + ", rc " + _rowNum + " lock " + colStart);
            if (_dir == Direction.LEFT) {
                List<int> newIndexList = new List<int> (ROW_SIZE - colStart);
                for (int i = colStart; i < ROW_SIZE; i++) {
                    int newIndex = i - _count;
                    if (newIndex < 0) {
                        newIndex = ROW_SIZE - Mathf.Abs (newIndex);
                    }

                    newIndexList.Add (newIndex);
                    Debug.Log ("i : " + i + ", newIndex " + newIndex);
                }

                List<int> newValueList = new List<int> (ROW_SIZE - colStart);
                for (int i = colStart; i < COL_SIZE; i++) {
                    for (int j = colStart; j < COL_SIZE; j++) {
                        if (newIndexList[j] == i) {
                            Debug.Log ("adding value at " + i + " at " + j);
                            newValueList.Add (mGridPoints[_rowNum, j]);
                            break;
                        }
                    }
                }

                for (int i = colStart; i < ROW_SIZE; i++) {
                    mGridPoints[_rowNum, i] = newValueList[i];
                }
            }
        }

        public void SwipeGrid (Direction _dir) {
            if (_dir == Direction.LEFT || _dir == Direction.RIGHT) {
                for (int w = 0; w < COL_SIZE; w++) {
                    int startIndex = _dir == Direction.LEFT ? 0 : COL_SIZE - 1;
                    int endIndex = _dir == Direction.LEFT ? COL_SIZE : -1;
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

            }

            PrintGrid ();
        }

        private void MergeSameValues (Direction _dir) {
            if (_dir == Direction.LEFT) {
                for (int w = 0; w < ROW_SIZE; w++) {
                    int rowIndex = w;
                    for (int i = 0; i < COL_SIZE; i++) {
                        if (mGridPoints[rowIndex, i] == 0) continue;
                        for (int j = i + 1; j < COL_SIZE; j++) {
                            if (mGridPoints[rowIndex, i] == mGridPoints[rowIndex, j]) {
                                mGridPoints[rowIndex, i]++;
                                mGridPoints[rowIndex, j] = 0;
                            }
                        }
                    }
                }
            } else if (_dir == Direction.RIGHT) {
                for (int w = ROW_SIZE - 1; w >= 0; w--) {
                    int rowIndex = w;
                    for (int i = COL_SIZE - 1; i >= 0; i--) {
                        if (mGridPoints[rowIndex, i] == 0) continue;
                        for (int j = i - 1; j >= 0; j--) {
                            if (mGridPoints[rowIndex, i] == mGridPoints[rowIndex, j]) {
                                mGridPoints[rowIndex, i]++;
                                mGridPoints[rowIndex, j] = 0;
                            }
                        }
                    }
                }
            }
        }

        void PrintGrid (bool _printUI = true) {
            StringBuilder sb = new StringBuilder ();
            for (int i = 0; i < 4; i++) {
                sb.Append ("row :: " + i + ">> ");
                for (int j = 0; j < 4; j++) {
                    sb.Append (mGridPoints[i, j] + " ");
                    if (!_printUI) continue;

                    int mapIndex = ((i + j) + ((4 - 1) * i));
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

        //input ssytem
        void Update () {

            if (Input.GetKeyDown (KeyCode.RightArrow)) {
                SwipeGrid (Direction.RIGHT);
                MergeSameValues (Direction.RIGHT);
                PrintGrid ();
            } else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
                SwipeGrid (Direction.LEFT);
                MergeSameValues (Direction.LEFT);
                PrintGrid ();
            } else if (Input.GetKeyDown (KeyCode.UpArrow)) {
                Debug.Log ("up arrow key");
                SwipeGrid (Direction.UP);
            } else if (Input.GetKeyDown (KeyCode.DownArrow)) {
                Debug.Log ("down key presed");
                SwipeGrid (Direction.DOWN);
            }
        }
    }
}