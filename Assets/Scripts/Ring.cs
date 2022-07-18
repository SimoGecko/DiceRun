// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;
using TMPro;

////////// PURPOSE: Represents a target that when hit gives you points //////////

namespace sxg
{
    public class Ring : MonoBehaviour // TODO: rename target
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
            TextFaceUpwards();
        }
    
        // -------------------- CUSTOM METHODS --------------------
    
    
        // commands
        void SetValue(int value)
        {
            this.value = Mathf.Clamp(value, 1, 6);
            tmp.text = value.ToString();
        }
        public void Used()
        {
            GameObject newEffect = Instantiate(collectEffect, transform.position, transform.rotation) as GameObject;
            GameObject.Destroy(newEffect, 10f);
            Destroy(gameObject);
        }

        void TextFaceUpwards()
        {
            tmp.transform.localEulerAngles = new Vector3(0f, 0f, Angle());
        }

        [EditorButton]
        void EDITOR_Setup()
        {
            SetValue(value);
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