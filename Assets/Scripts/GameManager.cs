// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

////////// PURPOSE: Controls the state of the game //////////

namespace sxg
{
    public class GameManager : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public event System.Action<Mode> OnModeChanged;
        public event System.Action OnGameStarted;
        
        public enum Mode { Menu, Game, Over}

        // private
        Mode mode;


        // references


        // -------------------- BASE METHODS --------------------

        void Start ()
        {
            SetMode(Mode.Menu);
        }
        
        void Update ()
        {
            if (mode == Mode.Menu)
            {
                if (StartInputPressed())
                {
                    SetMode(Mode.Game);
                }
                if (EscInputPressed())
                {
                    Quit();
                }
            }
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        public void SetMode(Mode newMode)
        {
            mode = newMode;
            OnModeChanged?.Invoke(mode);
            if (mode == Mode.Game) OnGameStarted?.Invoke();
        }

        public void RestartGame()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        public void Quit()
        {
            Application.Quit();
        }

        // queries
        bool StartInputPressed()
        {
            KeyCode[] codes = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Q, KeyCode.E, KeyCode.LeftShift, KeyCode.Space };
            foreach (KeyCode kc in codes) if (Input.GetKeyDown(kc)) return true;
            //if (Input.GetMouseButtonDown(0)) return true;
            return false;
        }
        bool EscInputPressed()
        {
            return Input.GetKeyDown(KeyCode.Escape);
        }
        public bool InGame { get { return mode == Mode.Game; } }



        // other
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<GameManager>();
                return instance;
            }
        }
    }
}