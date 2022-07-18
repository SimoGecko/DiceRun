// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;
using TMPro;

////////// PURPOSE:  //////////

namespace sxg
{
    public class Ring : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public int value;


        // private
        Transform cameraT;

        // references
        public TextMeshPro tmp;
        public GameObject collectEffect;
        
        // -------------------- BASE METHODS --------------------

        void Start ()
        {
            cameraT = Camera.main.transform;
        }
        
        void Update ()
        {
            // text faces upwards
            tmp.transform.localEulerAngles = new Vector3(0f, 0f, Angle());
        }
    
        // -------------------- CUSTOM METHODS --------------------
    
    
        // commands
        void SetValue(int value)
        {
            this.value = Mathf.Clamp(value, 1, 6);
            EDITOR_Setup();
        }
        public void Used()
        {
            // particles/effects
            GameObject newEffect = Instantiate(collectEffect, transform.position, transform.rotation) as GameObject;
            GameObject.Destroy(newEffect, 10f);
            Destroy(gameObject);
            // delete
        }
    
        [EditorButton]
        void EDITOR_Setup()
        {
            tmp.text = value.ToString();
        }

        public void SetRandom()
        {
            SetValue(Random.Range(1, 7));
        }

        // queries
        float Angle()
        {
            Vector3 projUp = Vector3.ProjectOnPlane(cameraT.up, transform.forward);
            return Vector3.SignedAngle(transform.up, projUp, transform.forward);
        }
    
    

        // other
    
    }
}