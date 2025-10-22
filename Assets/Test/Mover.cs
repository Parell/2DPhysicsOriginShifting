using UnityEngine;

namespace Decel
{
    public class Mover : MonoBehaviour
    {
        public Vector3 translation;
        public int mainThruster;
        private Body body;
        private float targetRotation;
        private Vector2d targetForce;

        private void Start()
        {
            body = GetComponent<Body>();
        }

        private void FixedUpdate()
        {
            // thruster allocator for 2D ships
            // Find all forces and torques by cross product
            // add to array 
            // Add pid controller again
            // Get ship to move to target locations
            // Add Ai functions to the vessel script
            // All thrusters have throttles and throttle up when given a command

            float controlThrusterForce = 1;
            targetRotation += translation.z * 50 * Time.fixedDeltaTime;
            targetForce = (Vector2d)translation * controlThrusterForce + Vector2d.up * 100 * mainThruster;

            body.AddAcceleration((Vector2d)transform.TransformVector((Vector3)targetForce));
            body.rb.rotation = targetRotation;
        }
    }
}
