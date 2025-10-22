using UnityEngine;

namespace Decel
{
    // decides were to point and were to go.
    public class Vessel : MonoBehaviour
    {
        public bool player;
        public bool controlable;
        //private bool retrograde;
        private float tapSpeed = 0.3f;
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
                    if (mover.mainThruster > 0)
                    {
                        mover.mainThruster = 0;
                    }
                    else
                    {
                        mover.mainThruster = 1;
                    }
                }
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdTime && !isHolding)
                {
                    isHolding = true;
                    mover.mainThruster = 2;
                }
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (isHolding)
                {
                    holdTimer = 0f;
                    isHolding = false;
                    mover.mainThruster = 0;
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

            bool qKey = Input.GetKey(KeyCode.Q);
            bool eKey = Input.GetKey(KeyCode.E);

            if (qKey)
            {
                mover.translation.z = 1;
            }
            if (eKey)
            {
                mover.translation.z = -1;
            }
            if ((!qKey && !eKey) || (qKey && eKey))
            {
                mover.translation.z = 0;
            }
        }
    }
}