// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;

////////// PURPOSE:  //////////

namespace sxg
{
    public class Difficulty : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public float logBase = 5;
    
        // private
    
    
        // references
        
        
        // -------------------- BASE METHODS --------------------

        void Start ()
        {
            
        }
        
        void Update ()
        {
            
        }
    
        // -------------------- CUSTOM METHODS --------------------
    
    
        // commands

    

    
        // queries
        public float GetMultiplier(float time)
        {
            time = Mathf.Max(time, 0f);
            return Mathf.Log(time + logBase, logBase);
        }


        // other
        private static Difficulty instance;
        public static Difficulty Instance
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<Difficulty>();
                return instance;
            }
        }
    }
}