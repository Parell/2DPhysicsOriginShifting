using UnityEngine;

namespace Decel
{
    [RequireComponent(typeof(Body))]
    public class CollisionResolver : MonoBehaviour
    {
        private Body mainBody;

        private void Awake()
        {
            mainBody = GetComponent<Body>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            ResolveCollision();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            ResolveCollision();
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (mainBody.bodyData.velocity.sqrMagnitude > 0.5)
            {
                PhysicsManager.Instance.ResetVelocity();
            }
        }

        private void ResolveCollision()
        {
            if (mainBody.rb.isKinematic) { return; }

            var deltaVelocity = (Vector2d)mainBody.rb.velocity - mainBody.bodyData.velocity;

            foreach (var body in PhysicsManager.bodies)
            {
                if (body.rb.isKinematic)
                {
                    body.bodyData.velocity -= deltaVelocity;
                }
                else
                {
                    body.rb.AddForce((Vector2)(deltaVelocity * -mainBody.bodyData.mass), ForceMode2D.Impulse);
                    body.bodyData.position = (Vector2d)body.rb.position;
                    body.bodyData.velocity = (Vector2d)body.rb.velocity;
                }
            }
        }
    }
}
