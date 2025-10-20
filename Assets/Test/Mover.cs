using UnityEngine;

namespace Decel
{
    public class Mover : MonoBehaviour
    {
        public Vector2 translation;
        private Body body;

        private void Start()
        {
            body = GetComponent<Body>();
        }

        private void FixedUpdate()
        {
            Vector2d force = Vector2d.Scale((Vector2d)translation, Vector2d.one * 100);

            body.AddAcceleration((Vector2d)transform.TransformVector((Vector3)force));
        }
    }
}
