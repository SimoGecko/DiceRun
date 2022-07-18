// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;

////////// PURPOSE:  //////////

namespace sxg
{
    public class Flyer : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public float lateralSpeed = 10f;
        public float maxTilt = 30f;
        public float smoothTime = 0.1f;
        public float forwardSpeed = 30f;
        public float shiftMultiplier = 1.3f;

        // private
        Vector2 inputSmooth;
        Vector2 smoothRef;
        float fwd = 1f;

        // references
        
        // -------------------- BASE METHODS --------------------

        void Start ()
        {
            fwd = 1f / Mathf.Tan(maxTilt * Mathf.Deg2Rad);
        }
        
        void Update ()
        {
            Vector2 input = GetDirectionInput();
            inputSmooth = Vector2.SmoothDamp(inputSmooth, input, ref smoothRef, smoothTime);
            transform.position += Time.deltaTime * (Vector3)inputSmooth * lateralSpeed;

            bool shift = GetBoostInput();
            float fwdSpeed = shift ? forwardSpeed * shiftMultiplier : forwardSpeed;
            transform.position += Time.deltaTime * Vector3.forward * fwdSpeed;


            Vector3 lookTarget = transform.position + Vector3.forward * fwd + (Vector3)inputSmooth;
            transform.LookAt(lookTarget, Vector3.up);
        }
    
        // -------------------- CUSTOM METHODS --------------------
    
    
        // commands
    

    
        // queries
        Vector2 GetDirectionInput()
        {
            Vector2 ans = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (ans.sqrMagnitude > 1f) ans.Normalize();
            return ans;
        }
        bool GetBoostInput()
        {
            return Input.GetKey(KeyCode.LeftShift);
        }


        // other
        private void OnDrawGizmos()
        {
        }
    }
}