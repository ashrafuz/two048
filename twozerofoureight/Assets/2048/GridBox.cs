using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Two048 {

    public class Ticker {
        private float frequencyTime;
        public float currentPassedTime;
        public bool isRunning;
        public Action actionToRun;

        public Ticker () {
            Reset ();
        }

        public virtual void Reset () {
            //Debug.Log ("resetting");
            isRunning = false;
            currentPassedTime = 0;
        }

        public void UpdateTick (float _dt) {
            currentPassedTime += _dt;
        }

        public void QueAction (Action _action) {
            actionToRun = _action;
        }

        public void Execute (bool _willPersist = false) {
            //Debug.Log ("executing..." + true + " frequency " + frequencyTime);
            if (actionToRun != null) {
                actionToRun ();
                actionToRun = _willPersist ? actionToRun : null;
                Reset ();
            }
        }

        public void SetFrequency (float _fr) {
            frequencyTime = _fr;
        }

        public float GetFrequency () {
            return frequencyTime;
        }

        public bool HasEnoughTimePassed () {
            return currentPassedTime > GetFrequency ();
        }

        public void Start () {
            isRunning = true;
        }
    }

    [System.Serializable]
    public struct ColorByValue {
        public int mValue;
        public Color mColor;
    }

    public class GridBox : MonoBehaviour {
        [SerializeField] private Text m_GridText;
        [SerializeField] private Image m_Bg;
        [SerializeField] private List<ColorByValue> colorBoxDB = new List<ColorByValue> ();
        [SerializeField] private float m_MinAnimLength = 0.5f;

        [SerializeField] private int mCurrentValue = 0;

        public void AnimateValue (int _s) {
            //Debug.Log ("animating " + _s);
            mCurrentValue = _s;
            m_GridText.text = mCurrentValue == 0 ? "" : mCurrentValue.ToString ();
            transform.localScale = Vector2.one * 1.2f;
            transform.DOScale (1, m_MinAnimLength);
            m_Bg.DOColor (GetColorByValue (mCurrentValue), m_MinAnimLength);
        }

        public void ModifyValue (int _s) {
            mCurrentValue = _s;
            m_GridText.transform.localScale = Vector2.one;
            m_GridText.text = _s == 0 ? "" : _s.ToString ();
            m_Bg.color = GetColorByValue (_s);
        }

        private Color GetColorByValue (int _val) {
            foreach (ColorByValue item in colorBoxDB) {
                if (item.mValue == _val) {
                    return item.mColor;
                }
            }
            return Color.black;
        }

        public void SetBgSize (float _newSize) {
            m_Bg.GetComponent<RectTransform> ().sizeDelta = new Vector2 (_newSize, _newSize);
        }

        public int GetCurrentValue () {
            return mCurrentValue;
        }
    }

}