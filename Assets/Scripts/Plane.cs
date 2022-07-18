// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;

////////// PURPOSE: Allows to control an object with plane-style controls //////////

namespace sxg
{
    public class Plane : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        [Header("Controls")]
        public float speed = 40f;
        public float pitchSpeed = 90f;
        public float yawSpeed = 90f;
        public float rollSpeed = -120f;

        public float pitchSmooth = 0.1f;
        public float rollSmooth = 0.1f;
        public float yawSmooth = 0.1f;

        [Header("Boost")]
        public float boostMultiplier = 2f;
        public float boostSmooth = 0.2f;

        public float boostCameraTrauma = 0.5f;

        // private
        float rollInput, rollRef;
        float pitchInput, pitchRef;
        float yawInput, yawRef;
        float boost = 1f, boostRef;

        bool exploded = false;

        // references


        // -------------------- BASE METHODS --------------------

        void Start ()
        {
        }
        
        void Update ()
        {
            if (GameManager.Instance.InGame && !exploded)
            {
                MovePlane();
                if(GetBoostInput())
                {
                    CameraManger.Instance.SetTrauma(boostCameraTrauma);
                }
            }
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        void MovePlane()
        {
            Vector2 inputDir = GetDirectionInput();
            float inputQE = GetInputQE();

            rollInput  = Mathf.SmoothDamp(rollInput,  inputQE,    ref rollRef,  rollSmooth);
            pitchInput = Mathf.SmoothDamp(pitchInput, inputDir.y, ref pitchRef, pitchSmooth);
            yawInput   = Mathf.SmoothDamp(yawInput,   inputDir.x, ref yawRef,   yawSmooth);

            bool boostInput = GetBoostInput();
            boost = Mathf.SmoothDamp(boost, boostInput ? boostMultiplier : 1f, ref boostRef, boostSmooth);

            transform.Rotate(new Vector3(0f, 0f, rollSpeed * rollInput * Time.deltaTime), Space.Self);

            transform.Rotate(new Vector3(pitchSpeed * pitchInput * Time.deltaTime, 0f, 0f), Space.Self);
            transform.Rotate(new Vector3(0f, yawSpeed * yawInput * Time.deltaTime, 0f), Space.Self);

            float speedMultiplier = Difficulty.Instance.GetMultiplier();
            transform.Translate(Vector3.forward * speed * boost * Time.deltaTime * speedMultiplier, Space.Self);
        }


        // queries
        Vector2 GetDirectionInput()
        {
            Vector2 ans = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (ans.sqrMagnitude > 1f) ans.Normalize();
            return ans;
        }
        float GetInputQE()
        {
            float ans = 0f;
            if (Input.GetKey(KeyCode.Q)) ans -= 1f;
            if (Input.GetKey(KeyCode.E)) ans += 1f;
            return ans;
        }
        bool GetBoostInput()
        {
            return Input.GetKey(KeyCode.LeftShift);
        }


        // other

    }
}