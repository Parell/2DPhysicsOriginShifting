using System;
using UnityEngine;

namespace Decel
{
    public class Vessel : MonoBehaviour
    {
        public bool player;
        public bool controlable;
        private float tapSpeed = 0.3f;
        //private bool retrograde;
        private float lastTapTime;
        private bool isHolding;
        private float holdTimer;
        private float holdTime = 0.5f;
        [HideInInspector] public Body body;
        [HideInInspector] public Mover mover;

        private void Start()
        {
            body = GetComponent<Body>();
            mover = GetComponent<Mover>();
        }

        private void Update()
        {
            if (controlable == false) { return; }

            if (player)
            {
                Inputs();
            }
        }

        private void Inputs()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (!isHolding)
                {
                    // if (mover.mainThrusterThrottle > 0)
                    // {
                    //     mover.mainThrusterThrottle = 2;
                    // }
                    // else
                    // {
                    //     //mover.mainThrusterThrottle = 0;
                    // }
                }
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdTime && !isHolding)
                {
                    isHolding = true;
                    // mover.mainThruster.acceleration = 100;
                    // mover.mainThruster.Calculate(body.bodyData.mass);
                    //mover.mainThrusterThrottle = 2;
                }
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (isHolding)
                {
                    holdTimer = 0f;
                    isHolding = false;
                    // mover.mainThruster.acceleration = 10;
                    // mover.mainThruster.Calculate(body.bodyData.mass);
                    // mover.mainEngine = false;
                    //mover.mainThrusterThrottle = 0;
                }
            }

            bool wKey = Input.GetKey(KeyCode.W);
            bool sKey = Input.GetKey(KeyCode.S);

            if (wKey)
            {
                mover.translation.y = 1;
                if (Input.GetKeyDown(KeyCode.W))
                {
                    if ((Time.time - lastTapTime) < tapSpeed)
                    {
                        //retrograde = false;
                    }
                    lastTapTime = Time.time;
                }
            }
            if (sKey)
            {
                mover.translation.y = -1;
                if (Input.GetKeyDown(KeyCode.S))
                {
                    if ((Time.time - lastTapTime) < tapSpeed)
                    {
                        //retrograde = true;
                    }
                    lastTapTime = Time.time;
                }
            }
            if ((!wKey && !sKey) || (wKey && sKey))
            {
                mover.translation.y = 0;
            }

            bool dKey = Input.GetKey(KeyCode.D);
            bool aKey = Input.GetKey(KeyCode.A);

            if (dKey)
            {
                mover.translation.x = 1;
            }
            if (aKey)
            {
                mover.translation.x = -1;
            }
            if ((!dKey && !aKey) || (dKey && aKey))
            {
                mover.translation.x = 0;
            }
        }
    }
}