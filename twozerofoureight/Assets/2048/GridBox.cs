using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Two048 {
    public class GridBox : MonoBehaviour {
        [SerializeField] private Text m_GridText;

        public void ModifyText (int _s) {
            m_GridText.text = _s.ToString ();
        }
    }

}