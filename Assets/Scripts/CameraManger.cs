// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;

////////// PURPOSE:  //////////

namespace sxg
{
    public class CameraManger : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public float smoothTime;
        public Vector3 offsetDir = new Vector3(0f, 4.3f, -10f);
        public float offsetLength = 10.9f;
        public Vector3 offsetRot;
        public bool relative = false;

        [Header("Screen Shake")]
        public bool doScreenShake = true;
        public float maxCameraShakeTranslation = 0.7f;
        public float maxCameraShakeAngle = 2f;
        public float traumaDecreaseSpeed = 1f;
        public float shakeFrequency = 30f;
        public float addTraumaAmount = 0.5f;

        public float distanceToHalfTrauma = 4f;
        [Range(0f, 1f)] public float traumaAmount;


        // private
        Vector3 smoothRef;
        Quaternion deriv;
        Vector3 virginPosition;

        // references
        public Transform cameraTransform;
        public Transform target;
        
        // -------------------- BASE METHODS --------------------

        Vector3 offset { get { return offsetDir.normalized * offsetLength; } }

        void Start ()
        {
            if (offset == Vector3.zero && target != null)
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
            //if (GameManager.Instance.InGame || GameManager.Instance.mode == GameManager.Mode.Menu)
            {
                FollowCamera();
            }
            Shakecamera();
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        void FollowCamera()
        {
            virginPosition = Vector3.SmoothDamp(virginPosition, TargetPos, ref smoothRef, smoothTime);
            cameraTransform.position = virginPosition;
            cameraTransform.rotation = SmoothDamp(cameraTransform.rotation, target.rotation, ref deriv, smoothTime);
        }

        void Shakecamera()
        {
            float amount = CameraShakeAmount();
            float angle = maxCameraShakeAngle * amount * PerlinShake(1.5f);
            Vector2 offset = Vector2.zero;
            offset.x = maxCameraShakeTranslation * amount * PerlinShake(2.5f);
            offset.y = maxCameraShakeTranslation * amount * PerlinShake(3.5f);

            cameraTransform.position += (Vector3)offset;
            //cameraOffsetTransform.position += (Vector3)offset; // this is repeated a lot
            //cameraOffsetTransform.eulerAngles = new Vector3(0f, 0f, angle);
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
        Vector3 TargetPos
        {
            get
            {
                if (!relative)
                {
                    return target.position + offset;
                }
                else
                {
                    //targetPos = target.position + target.TransformVector(offset);
                    //targetPos = target.position + target.rotation * offset;
                    return target.TransformPoint(offset);
                }
            }
        }

        private float CameraShakeAmount()
        {
            return Mathf.Pow(traumaAmount, 2f);
        }
        float PerlinShake(float seed)
        {
            return Mathf.PerlinNoise(seed, Time.time * shakeFrequency) * 2f - 1f;
        }

        public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
        {
            if (Time.deltaTime < Mathf.Epsilon) return rot;
            // account for double-cover
            var Dot = Quaternion.Dot(rot, target);
            var Multi = Dot > 0f ? 1f : -1f;
            target.x *= Multi;
            target.y *= Multi;
            target.z *= Multi;
            target.w *= Multi;
            // smooth damp (nlerp approx)
            var Result = new Vector4(
            Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
            Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
            Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
            Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
        ).normalized;

            // ensure deriv is tangent
            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(Result.x, Result.y, Result.z, Result.w);
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