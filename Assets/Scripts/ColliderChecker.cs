// (c) Simone Guggiari 2022

using System.Collections.Generic;
using UnityEngine;

////////// PURPOSE:  //////////

namespace sxg
{
    public class ColliderChecker : MonoBehaviour
    {
        // -------------------- VARIABLES --------------------

        // public
        public float circleRadius = 1f;
        public float explosionCameraTrauma = 0.5f;
        public float ringCollectedCameraTrauma = 0.5f;

        // private
        LevelGenerator.Circle circle0, circle1;
        bool exploded = false;

        // references
        public ScoreManager scorem;
        public DiceRoll diceroll;

        public GameObject explosionParticles;
        public GameObject objectToHide;

        // -------------------- BASE METHODS --------------------

        private void Awake()
        {
            //LevelGenerator.Instance.OnLevelCreated
            FindObjectOfType<RethrowForcer>().OnRethrowFailed += Explode;
        }

        void Start ()
        {
            circle0 = LevelGenerator.Instance.GetNextCircle();
            circle1 = LevelGenerator.Instance.GetNextCircle();
        }
        
        void Update ()
        {
            
        }
        private void LateUpdate()
        {
            // after we moved
            if (GameManager.Instance.InGame)
            {
                while (GotoNextCircle(transform.position))
                {
                    //Debug.Log("goto next circle");
                    circle0 = circle1;
                    circle1 = LevelGenerator.Instance.GetNextCircle();
                }
                if (IsOutsideCircles(transform.position))
                {
                    Explode();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "ring")
            {
                Ring ring = other.GetComponent<Ring>();
                if (ring.value == diceroll.number)
                {
                    // add score
                    scorem.AddScore(ring.value);
                    CameraManger.Instance.AddTrauma(ringCollectedCameraTrauma);
                    ring.Used();
                }
                else
                {
                    Explode();
                }
            }
            else if (other.tag == "obstacle")
            {
                Explode();
            }
        }

        // -------------------- CUSTOM METHODS --------------------


        // commands
        void Explode()
        {
            if (exploded) return;
            exploded = true;

            CameraManger.Instance.AddTrauma(explosionCameraTrauma);


            objectToHide.SetActive(false);
            Instantiate(explosionParticles, objectToHide.transform.position, objectToHide.transform.rotation);
            //this.Invoke(nameof(SetGameOver), 1f);
            SetGameOver();
        }

        void SetGameOver()
        {
            GameManager.Instance.SetMode(GameManager.Mode.Over);
        }


        // queries
        bool IsOutsideCircles(Vector3 point)
        {
            Vector3 toCircle = circle1.center - circle0.center;
            Vector3 delta = point - circle0.center;
            Vector3 deltaP = Vector3.Project(delta, toCircle);
            Vector3 deltaT = delta - deltaP;
            float t = Mathf.Clamp01(deltaP.magnitude / toCircle.magnitude);
            return deltaT.magnitude > Mathf.Lerp(circle0.radius, circle1.radius, t) - circleRadius;
        }

        bool GotoNextCircle(Vector3 point)
        {
            return Vector3.Dot(circle1.orientation * Vector3.forward, point-circle1.center) >= 0f;
        }



        // other
        private void OnDrawGizmos()
        {
            //if (circle0 != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos2.DrawWireCircle(circle0.center, circle0.radius, circle0.orientation * Vector3.forward);
            }
        }
    }
}