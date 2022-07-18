// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

////////// PURPOSE:  //////////

namespace sxg
{
    public class ScoreManager : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public


        // private
        int score;
        float startTime;
        int timeMs;
        bool submittedHighscore = false;


        // references
        public RectTransform menuPanel, gamePanel, overPanel, gameandOverPanel;
        public TextMeshProUGUI scoreText, timeText, numberText, addText;

        public TextMeshProUGUI highscoreRank, highscoreNames, highscoreScores, highscoresTime;
        public TMP_InputField usernameField;

        // -------------------- BASE METHODS --------------------
        private void Awake()
        {
            Highscores.Instance.OnDownload += OnHighscoresDownload;
            Highscores.Instance.OnUpload += OnHighscoresUploaded;

            GameManager.Instance.OnModeChanged += OnModeChanged;
            FindObjectOfType<DiceRoll>().OnDiceNumber += OnDiceNumber;
            numberText.gameObject.transform.localScale = Vector3.zero;
            addText.gameObject.transform.localScale = Vector3.zero;

            highscoreRank.text = "";
            highscoreNames.text = "";
            highscoreScores.text = "";
            highscoresTime.text = "";
        }

        void Start ()
        {

        }
        
        void Update ()
        {
            if (GameManager.Instance.InGame)
            {
                TimeSpan ts = TimeSpan.FromSeconds(Time.realtimeSinceStartup -startTime);
                string times = string.Format("{0:D2}:{1:D2}:{2:D3}", ts.Minutes, ts.Seconds, ts.Milliseconds);
                timeText.text = $"Time: {times}";
            }
        }


        // -------------------- CUSTOM METHODS --------------------


        // commands
        public void SubmitHighscore()
        {
            if (submittedHighscore || usernameField.text.IsNullOrEmpty()) return;
            submittedHighscore = true;
            Highscores.Instance.UploadEntry(usernameField.text, score, timeMs);
        }

        void OnModeChanged(GameManager.Mode mode)
        {
            if (mode == GameManager.Mode.Menu)
            {
                menuPanel.GetComponent<CanvasGroup>().alpha = 1f;
            }
            if (mode == GameManager.Mode.Game)
            {
                score = 0;
                startTime = Time.realtimeSinceStartup;
                AddScore(0);
                //iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.5f, "delay", 0f,
                //    "easetype", "easeInOutSine", "onupdate", nameof(Fade2)));
            }
            else if (mode == GameManager.Mode.Over)
            {
                TimeSpan ts = TimeSpan.FromSeconds(Time.realtimeSinceStartup - startTime);
                timeMs = Mathf.RoundToInt((float)ts.TotalMilliseconds);
                ts = TimeSpan.FromMilliseconds(timeMs);
                string times = string.Format("{0:D2}:{1:D2}:{2:D3}", ts.Minutes, ts.Seconds, ts.Milliseconds);
                timeText.text = $"Time: {times}";

                Highscores.Instance.DownloadEntries();

                scoreText.text = $"Score: {score}";

                // fade in for overpanel
                overPanel.GetComponent<CanvasGroup>().alpha = 0f;
                iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 1f, "delay", 0.5f,
                    "easetype", "easeInOutSine", "onupdate", nameof(Fade)));
            }
            menuPanel.gameObject.SetActive(mode == GameManager.Mode.Menu);// || mode == GameManager.Mode.Game);
            gamePanel.gameObject.SetActive(mode == GameManager.Mode.Game);
            gameandOverPanel.gameObject.SetActive(mode == GameManager.Mode.Game || mode == GameManager.Mode.Over);
            overPanel.gameObject.SetActive(mode == GameManager.Mode.Over);
        }

        void Fade(float value)
        {
            overPanel.GetComponent<CanvasGroup>().alpha = value;
        }
        void Fade2(float value)
        {
            menuPanel.GetComponent<CanvasGroup>().alpha = 1f-value;
        }

        public void AddScore(int value)
        {
            if (!GameManager.Instance.InGame) return;
            score += value;
            // update ui
            scoreText.text = $"Score: {score}";

            addText.text = "+" + value.ToString();
            // itween
            addText.gameObject.transform.localScale = Vector3.zero;
            iTween.ScaleTo(addText.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.7f, "delay", 0f, "easetype", "easeOutElastic"));
            iTween.ScaleTo(addText.gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.2f, "delay", .7f));
        }

        void OnDiceNumber(int number)
        {
            numberText.text = number.ToString();
            // itween
            numberText.gameObject.transform.localScale = Vector3.zero;
            iTween.ScaleTo(numberText.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.7f, "delay", 0f, "easetype", "easeOutElastic"));
            iTween.ScaleTo(numberText.gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.2f, "delay", .7f));
        }

        void OnHighscoresUploaded()
        {
            Highscores.Instance.DownloadEntries(); // to re-sync the list
        }
        void OnHighscoresDownload()
        {
            highscoreRank.text   = Highscores.Instance.RankString();
            highscoreNames.text  = Highscores.Instance.UserString();
            highscoreScores.text = Highscores.Instance.ScoreString();
            highscoresTime.text  = Highscores.Instance.TimeString();
        }

        // queries
        public float WallTimeSeconds { get { TimeSpan ts = TimeSpan.FromSeconds(Time.realtimeSinceStartup - startTime); return (float)ts.TotalSeconds; } }



        // other
        private static ScoreManager instance;
        public static ScoreManager Instance
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<ScoreManager>();
                return instance;
            }
        }
    }
}