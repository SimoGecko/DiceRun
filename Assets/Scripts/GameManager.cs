// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

////////// PURPOSE:  //////////

namespace sxg
{
    public class GameManager : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public event System.Action<Mode> OnModeChanged;
        
        public enum Mode { Menu, Game, Over}
        public Mode mode;
    
        // private
    
    
        // references
        
        
        // -------------------- BASE METHODS --------------------

        void Start ()
        {
            SetMode(Mode.Menu);
        }
        
        void Update ()
        {
            if (mode == Mode.Menu && RelevantInputPressed()) SetMode(Mode.Game);
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        public void SetMode(Mode newMode)
        {
            this.mode = newMode;
            OnModeChanged?.Invoke(mode);
        }

        public void RestartGame()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        // queries
        bool RelevantInputPressed()
        {
            KeyCode[] codes = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Q, KeyCode.E, KeyCode.LeftShift, KeyCode.Space };
            foreach (KeyCode kc in codes) if (Input.GetKeyDown(kc)) return true;
            if (Input.GetMouseButtonDown(0)) return true;
            return false;
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