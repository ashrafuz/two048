using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Two048 {

    public class G2048Util {

        public const string GAME_NAME = "g2048";
        public const string GAME_RESTART_EVENT = "g2048_restart";
        public const string GAMEOVER_RESTART_EVENT = "g2048_game_over_restart";
        public const string GAME_START = "g2048_match_start";
        public const string GAME_SCENE_NAME = "two048_game_scene";
        public const string GAME_MATCH_COMPLETE = "g2048_match_complete";

    }

    public class Two048UIManager : MonoBehaviour {

        [SerializeField] private Button m_RestartBtn;
        [SerializeField] private GridView m_GridView;
        [SerializeField] private Text m_CurrentScoreText;
        [SerializeField] private Text m_HighScoreText;
        [SerializeField] private GameObject m_GameOverPanel;
        [SerializeField] private Button m_GameOverRestartBtn;

        [SerializeField] private int mCurrentScore = 0;
        [SerializeField] private int mHighScore = 0;

        private const string HIGH_SCORE_KEY = "g_2048_high_score";

        void Start () {

            m_RestartBtn.onClick.RemoveAllListeners ();
            m_RestartBtn.onClick.AddListener (() => {
                SceneManager.LoadScene (G2048Util.GAME_SCENE_NAME);
            });

            m_GameOverRestartBtn.onClick.RemoveAllListeners ();
            m_GameOverRestartBtn.onClick.AddListener (() => {
                SceneManager.LoadScene (G2048Util.GAME_SCENE_NAME);
            });

            mHighScore = PlayerPrefs.GetInt (HIGH_SCORE_KEY, 0);
            m_CurrentScoreText.text = "Score \n" + mCurrentScore;
            m_HighScoreText.text = "Highscore \n " + mHighScore;

            m_GridView.OnScore += OnScoreUpdate;
            m_GridView.OnGameOver += OnGameOverUI;
        }

        private void OnScoreUpdate (int _score) {
            mCurrentScore += (int) (Mathf.Pow (2, _score));
            if (mCurrentScore > mHighScore) {
                mHighScore = mCurrentScore;
                SaveScoreToPref ();
            }

            m_CurrentScoreText.text = "Score \n" + mCurrentScore;
            m_HighScoreText.text = "Highscore \n " + mHighScore;
        }

        private void OnGameOverUI () {
            //m_GameOverPanel.SetActive (true);
        }

        private void SaveScoreToPref () {
            PlayerPrefs.SetInt (HIGH_SCORE_KEY, mHighScore);
        }
    }
}