using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Two048 {

    [System.Serializable]
    public struct ColorByValue {
        public int mValue;
        public Color mColor;
    }

    public class GridBox : MonoBehaviour {
        [SerializeField] private Text m_GridText;
        [SerializeField] private Image m_Bg;
        [SerializeField] private List<ColorByValue> colorBoxDB = new List<ColorByValue> ();

        public void ModifyText (int _s) {
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
    }

}