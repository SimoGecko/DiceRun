// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;

////////// PURPOSE:  //////////
namespace sxg
{ 
    public class DiceRoll : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public event System.Action OnDiceRoll;
        public event System.Action<int> OnDiceNumber;

        public Vector2 torqueRange = new Vector2(3000f, 5000f);
        public Vector3 startPos;
        public float forceUp = 500f;
        public float gravity = -10f;
        public float maxAngularVelocity = 1000f;

        public float stoppedThreshold = 0.1f;

        public int number = 0;

        // private
        bool mustRoll = false;
    
        // references
        public Rigidbody rb;


        // -------------------- BASE METHODS --------------------

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            Physics.gravity = new Vector3(0f, gravity, 0f);
            rb.maxAngularVelocity = maxAngularVelocity;

            GameManager.Instance.OnModeChanged += OnModeChanged;
        }

        void Start ()
        {

        }
        
        void Update ()
        {
            if (GameManager.Instance.InGame && Input.GetKeyDown(KeyCode.Space))
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
            //rb.AddForce(Vector3.down * gravity);
            else if (number == 0 && IsStopped(rb))
            {
                // check the face up
                number = GetDiceNumber();
                OnDiceNumber?.Invoke(number);
                //Debug.Log("computing number " + number);
            }
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        void OnModeChanged(GameManager.Mode mode)
        {
            if(mode == GameManager.Mode.Game)
            {
                mustRoll = true;
            }
        }

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
            return rb.position.y <= 0.55f && rb.velocity.sqrMagnitude <= stoppedThreshold && rb.angularVelocity.sqrMagnitude <= stoppedThreshold;
        }

        public static Quaternion RandomQuaternion()
        {
            //return Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f);
            float u = Random.value;
            float v = Random.value;
            float w = Random.value;
            float twoPi = Mathf.PI * 2f;
            return new Quaternion(Mathf.Sqrt(1 - u) * Mathf.Sin(twoPi*v), Mathf.Sqrt(1f - u) * Mathf.Cos(twoPi*v), Mathf.Sqrt(u) * Mathf.Sin(twoPi*w), Mathf.Sqrt(u) * Mathf.Cos(twoPi*w));
        }

        int GetDiceNumber()
        {
            if (transform.forward.y > 0.7f) return 1;
            if (transform.right.y > 0.7f) return 2;
            if (-transform.up.y > 0.7f) return 3;
            if (transform.up.y > 0.7f) return 4;
            if (-transform.right.y > 0.7f) return 5;
            if (-transform.forward.y > 0.7f) return 6;
            return 0;
        }


        // other

    }
}