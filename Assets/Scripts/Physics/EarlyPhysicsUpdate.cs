using UnityEngine;

namespace Decel
{
    [DefaultExecutionOrder(-10)]
    public class EarlyPhysicsUpdate : MonoBehaviour
    {
        private void FixedUpdate()
        {
            foreach (var body in PhysicsManager.bodies)
            {
                if (body.bodyData.hasAcceleration)
                {
                    body.hasAccelerationTimer -= PhysicsManager.deltaTime;
                    if (body.hasAccelerationTimer < 0)
                    {
                        body.bodyData.hasAcceleration = false;
                        body.hasAccelerationTimer = 0;
                    }
                }
                body.bodyData.acceleration = Vector2d.zero;
            }
        }
    }
}
