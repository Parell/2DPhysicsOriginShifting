using UnityEngine;

namespace Decel
{
    [ExecuteAlways, RequireComponent(typeof(Rigidbody2D))]
    public class Body : MonoBehaviour
    {
        public BodyData bodyData = new BodyData();
        [HideInInspector] public float hasAccelerationTimer;
        [HideInInspector] public Rigidbody2D rb;
#if UNITY_EDITOR
        private Vector2 lastTransformPostion;
        private Vector2d lastBodyDataPostion;

        private void Update()
        {
            if (!Application.isEditor && Application.isPlaying) { return; }

            if ((Vector2)transform.position != lastTransformPostion)
            {
                lastTransformPostion = transform.position;
                bodyData.position = (Vector2d)transform.position;
            }
            else if (bodyData.position != lastBodyDataPostion)
            {
                lastBodyDataPostion = bodyData.position;
                transform.position = (Vector2)bodyData.position;
            }
        }
#endif

        public void AddAcceleration(Vector2d acceleration)
        {
            if (acceleration.sqrMagnitude > 0)
            {
                bodyData.hasAcceleration = true;
                hasAccelerationTimer = 2;
                bodyData.acceleration += acceleration;
            }
        }

        public void Collisions(bool state)
        {
            if (state)
            {
                if (rb.isKinematic)
                {
                    rb.mass = (float)bodyData.mass;
                    rb.velocity = (Vector2)bodyData.velocity;
                    rb.position = (Vector2)bodyData.position;
                    rb.rotation = bodyData.rotation;
                    rb.angularVelocity = bodyData.angularVelocity;
                }
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            else
            {
                if (!rb.isKinematic)
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.isKinematic = true;
                }
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
        }
    }
}