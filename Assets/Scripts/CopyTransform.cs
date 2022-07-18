// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;

////////// PURPOSE: General-purpose script to copy the transform of another transform //////////

namespace sxg
{
    public class CopyTransform : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public Vector3 offsetPos;
        public Vector3 offsetRot;
        public bool copyPos = false;
        public bool copyRot = false;
        public bool local = false;
    
        // private
    
    
        // references
        public Transform source;
        
        
        // -------------------- BASE METHODS --------------------

        void Start ()
        {
            
        }
        
        void Update ()
        {
            
        }

        private void LateUpdate()
        {
            if (source != null)
            {
                if (local)
                {
                    if (copyPos) transform.localPosition = source.localPosition + offsetPos;
                    if (copyRot) transform.localRotation = Quaternion.Euler(offsetRot) * source.localRotation;
                }
                else
                {
                    if (copyPos) transform.position = source.position + offsetPos;
                    if (copyRot) transform.rotation = Quaternion.Euler(offsetRot) * source.rotation;
                }
            }
        }
        // -------------------- CUSTOM METHODS --------------------


        // commands



        // queries



        // other

    }
}