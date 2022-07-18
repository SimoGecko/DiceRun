// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;

////////// PURPOSE: Used to simulate the roll of a dice when pressing a button //////////
namespace sxg
{ 
    public class DiceRoll : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public event System.Action OnDiceRoll;
        public event System.Action<int> OnDiceNumber;

        public Vector2 torqueRange = new Vector2(1f, 4f);
        public Vector3 startPos = new Vector3(0f, 2f, 0f);
        public float forceUp = 1000f;
        public float gravity = -70f;
        public float maxAngularVelocity = 100f;

        public float velocityStoppedThreshold = 0.1f;
        public float posYstoppedThreshold = 0.55f;
        public int number = 0;

        public KeyCode rollKeycode = KeyCode.Space;

        // private
        bool mustRoll = false;
    
        // references
        Rigidbody rb;


        // -------------------- BASE METHODS --------------------

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            Physics.gravity = new Vector3(0f, gravity, 0f);
            rb.maxAngularVelocity = maxAngularVelocity;

            GameManager.Instance.OnGameStarted += () => mustRoll = true;
        }

        void Start ()
        {

        }
        
        void Update ()
        {
            if (GameManager.Instance.InGame && Input.GetKeyDown(rollKeycode))
            {
                mustRoll = true;
            }
        }

        private void FixedUpdate()
        {
            if (mustRoll)
            {
                mustRoll = false;
                Roll();
            }
            if (number == -1) number = 0;
            else if (number == 0 && IsStopped(rb))
            {
                number = GetDiceNumber();
                OnDiceNumber?.Invoke(number);
            }
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        void Roll()
        {
            OnDiceRoll?.Invoke();
            //Vector3 dir = new Vector3(Random.value, Random.value, Random.value) * torque;
            transform.position = startPos;
            //transform.rotation = RandomQuaternion();
            rb.AddForce(Vector3.up * forceUp);
            Vector3 torque = Random.onUnitSphere * Random.Range(torqueRange.x, torqueRange.y);
            rb.AddTorque(torque, ForceMode.Impulse);
            number = -1;
        }


        // queries
        bool IsStopped(Rigidbody rb)
        {
            //return rb.IsSleeping(); // works but too slow
            return (rb.position.y <= posYstoppedThreshold) && (rb.velocity.sqrMagnitude <= velocityStoppedThreshold) && (rb.angularVelocity.sqrMagnitude <= velocityStoppedThreshold);
        }

        int GetDiceNumber()
        {
            float threshold = 0.8f;
            if ( transform.forward.y >= threshold) return 1;
            if ( transform.right.y   >= threshold) return 2;
            if (-transform.up.y      >= threshold) return 3;
            if ( transform.up.y      >= threshold) return 4;
            if (-transform.right.y   >= threshold) return 5;
            if (-transform.forward.y >= threshold) return 6;
            Debug.LogWarning("Could not find a side up");
            return 0;
        }


        // other

    }
}