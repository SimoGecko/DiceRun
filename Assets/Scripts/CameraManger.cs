// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;

////////// PURPOSE: Deals with the camera behavior, following the target and providing screen shake //////////

namespace sxg
{
    public class CameraManger : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        [Header("Follow")]
        public float smoothTime;
        public Vector3 offsetDir = new Vector3(0f, 4.3f, -10f);
        public float offsetLength = 10.9f;
        public bool relative = false;

        [Header("Screen Shake")]
        public bool doScreenShake = true;
        public float maxCameraShakeTranslation = 0.7f;
        public float maxCameraShakeAngle = 2f;
        public float traumaDecreaseSpeed = 1f;
        public float shakeFrequency = 30f;
        public float addTraumaAmount = 0.5f;
        public float distanceToHalfTrauma = 4f;


        // private
        Vector3 smoothRef;
        Quaternion deriv;
        Vector3 virginPosition;
        float traumaAmount;

        // references
        [Header("References")]
        public Transform cameraTransform;
        public Transform target;
        
        // -------------------- BASE METHODS --------------------


        void Start ()
        {
            if (Offset == Vector3.zero && target != null)
            {
                Vector3 newOffset = cameraTransform.position - target.position;
                offsetDir = newOffset.normalized;
                offsetLength = newOffset.magnitude;
            }
            virginPosition = TargetPos;
            cameraTransform.position = virginPosition;
        }
        
        void Update ()
        {
            if (traumaAmount > 0f)
            {
                traumaAmount = Mathf.Clamp01(traumaAmount - Time.deltaTime * traumaDecreaseSpeed);
            }
        }

        private void LateUpdate()
        {
            FollowCamera();
            Shakecamera();
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        void FollowCamera()
        {
            virginPosition = Vector3.SmoothDamp(virginPosition, TargetPos, ref smoothRef, smoothTime);
            cameraTransform.position = virginPosition;
            cameraTransform.rotation = Utility.SmoothDamp(cameraTransform.rotation, target.rotation, ref deriv, smoothTime);
        }

        void Shakecamera()
        {
            float amount = CameraShakeAmount();
            Vector2 offset = Vector2.zero;
            offset.x = maxCameraShakeTranslation * amount * PerlinShake(2.5f);
            offset.y = maxCameraShakeTranslation * amount * PerlinShake(3.5f);

            cameraTransform.position += (Vector3)offset;
        }

        public void AddTrauma(float value)
        {
            traumaAmount = Mathf.Clamp01(traumaAmount + value);
        }
        public void SetTrauma(float value)
        {
            traumaAmount = Mathf.Clamp01(value);
        }

        // queries
        private Vector3 Offset { get { return offsetDir.normalized * offsetLength; } }

        private Vector3 TargetPos { get { return relative ? target.TransformPoint(Offset) : target.position + Offset; } }

        private float CameraShakeAmount()
        {
            return Mathf.Pow(traumaAmount, 2f);
        }
        float PerlinShake(float seed)
        {
            return Mathf.PerlinNoise(seed, Time.time * shakeFrequency) * 2f - 1f;
        }




        // other
        private static CameraManger instance;
        public static CameraManger Instance
        {
            get
            {
                if (instance == null) instance = FindObjectOfType<CameraManger>();
                return instance;
            }
        }
    }
}