// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

////////// PURPOSE: Forces the player to rethrow the dice after a certain time or explodes otherwise //////////

namespace sxg
{
    public class RethrowForcer : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        public event System.Action OnRethrowFailed;
        // public
        public float rethrowTime = 10f;

        // private
        private float remainingTime;
        bool exploded;


        // references
        public RectTransform sizer;
        
        // -------------------- BASE METHODS --------------------


        void Awake ()
        {
            GetComponent<DiceRoll>().OnDiceRoll += OnRoll;
            exploded = false;
            remainingTime = rethrowTime;
        }
        
        void Update ()
        {
            if (GameManager.Instance.InGame && !exploded)
            {
                remainingTime = Mathf.Clamp(remainingTime - Time.deltaTime, 0f, rethrowTime);
                if (remainingTime <= 0f)
                {
                    exploded = true;
                    OnRethrowFailed?.Invoke();
                }
                sizer.localScale = new Vector3(TimeLeftPercent, 1f, 1f);
            }
        }
    
        // -------------------- CUSTOM METHODS --------------------
    
    
        // commands
        void OnRoll()
        {
            remainingTime = rethrowTime;
        }

    
        // queries
        public float TimeLeftPercent { get { return remainingTime / rethrowTime; } }
    

        // other
    
    }
}