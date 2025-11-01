using UnityEngine;

namespace Decel
{
    public class Mover : MonoBehaviour
    {
        public Vector3 translation;
        public int mainThruster;
        private Body body;
        public float maxAngularVelocity;

        [SerializeField] private float targetRotation;
        [SerializeField] private float deltaRotation;
        [SerializeField] private float wantedTorque;

        [SerializeField] private Vector2 targetPosition;
        [SerializeField] private Vector2 wantedForce;

        private Thruster[] thrusters;
        [SerializeField] private bool thrustersDirty = true;

        private float[,] J;
        private float[] uMin;
        private float[] uMax;
        [SerializeField] private Vector2 minTorqueSaturation;
        [SerializeField] private Vector2 maxTorqueSaturation;

        private void Start()
        {
            body = GetComponent<Body>();
            thrusters = FindObjectsOfType<Thruster>(false);
            thrustersDirty = true;
        }

        private void FindThrusters()
        {
            thrusters = FindObjectsOfType<Thruster>(false);

            int n = thrusters.Length;
            J = new float[n, 3];
            uMin = new float[n];
            uMax = new float[n];

            for (int i = 0; i < n; i++)
            {
                var thr = thrusters[i];

                uMin[i] = 0;
                uMax[i] = thr.maxForce;

                thr.direction = (Vector2)(thr.transform.localRotation * Vector2.up).normalized;
                thr.position = body.rb.centerOfMass - (Vector2)thr.transform.localPosition;

                J[i, 0] = thr.direction.x;
                J[i, 1] = thr.direction.y;
                J[i, 2] = thr.position.x * thr.direction.y - thr.position.y * thr.direction.x;

                // float minTorque = thr.position.x * thr.direction.y * thr.minForce - thr.position.y * thr.direction.x * thr.minForce;
                // float maxTorque = thr.position.x * thr.direction.y * thr.maxForce - thr.position.y * thr.direction.x * thr.maxForce;
                // if (maxTorque > 0) { minTorqueSaturation.x += minTorque; maxTorqueSaturation.x += maxTorque; } // left
                // if (maxTorque < 0) { minTorqueSaturation.y += minTorque; maxTorqueSaturation.y += maxTorque; } // right
            }
        }

        private void FixedUpdate()
        {
            if (thrustersDirty)
            {
                FindThrusters();
                thrustersDirty = false;
            }

            float maxTorque = 100f;
            float minTorque = 50f;
            float omegaCoast = maxAngularVelocity * Mathf.Deg2Rad;
            float zeta = 1f;
            float wn = 2f;
            float kp = wn * wn;
            float kd = 2f * zeta * wn;

            deltaRotation = Mathf.DeltaAngle(body.rb.rotation, targetRotation) * Mathf.Deg2Rad;

            float omega = body.rb.angularVelocity * Mathf.Deg2Rad;

            float maxAngularAcceleration = maxTorque / body.rb.inertia;
            float idealBrakeDistance = omega * omega / (2f * maxAngularAcceleration);

            if (Mathf.Abs(deltaRotation) <= idealBrakeDistance) { wantedTorque = -Mathf.Sign(omega) * maxTorque; }
            else if (Mathf.Abs(deltaRotation) > idealBrakeDistance && Mathf.Abs(omega) < omegaCoast)
            {
                float alphaCmd = kp * deltaRotation - kd * omega;
                wantedTorque = Mathf.Clamp(body.rb.inertia * alphaCmd, -maxTorque, maxTorque);
            }
            else { wantedTorque = 0f; }

            if (Mathf.Abs(wantedTorque) < minTorque) { wantedTorque = 0f; }

            float maxForce = 50f;
            float minForce = 1f;
            float vCoast = 100f;
            float zeta2 = 1f;
            float wn2 = 2f;
            float kp2 = wn2 * wn2;
            float kd2 = 2f * zeta2 * wn2;

            //body.rb.position - targetPosition
            Vector2 deltaPos = Vector2.zero;
            Vector2 v = body.rb.velocity;

            float m = body.rb.mass;
            float maxAccel = maxForce / m;
            float idealBrakePositionDistance = v.sqrMagnitude / (2f * maxAccel);

            if (deltaPos.magnitude <= idealBrakePositionDistance) { wantedForce = -v.normalized * maxForce; }
            else if (deltaPos.magnitude > idealBrakePositionDistance && v.magnitude < vCoast)
            {
                Vector2 aCmd = kp2 * deltaPos - kd2 * v;
                wantedForce = Vector2.ClampMagnitude(m * aCmd, maxForce);
            }
            else { wantedForce = Vector2.zero; }

            if (wantedForce.magnitude < minForce) { wantedForce = Vector2.zero; }

            Vector3 desired = new Vector3(wantedForce.x, wantedForce.y, wantedTorque);

            float[] u = ThrusterAllocator2D.Allocate(J, desired, null, uMin, uMax, maxIters: 4, lambda: 1e-3f);

            for (int i = 0; i < u.Length; i++)
            {
                thrusters[i].throttle = u[i];

                Vector2 force = thrusters[i].direction * u[i];
                Vector2 r = thrusters[i].position - body.rb.worldCenterOfMass;
                body.rb.AddForce(force);
                body.rb.AddTorque(r.x * force.y - r.y * force.x);
            }
        }

        private void Update()
        {
            targetRotation += translation.z * Time.fixedDeltaTime * (360 / 4);
        }
    }
}
