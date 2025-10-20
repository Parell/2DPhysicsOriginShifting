using UnityEngine;

namespace Decel
{
    public class Test : MonoBehaviour
    {
        public Vector2d relpos;
        public Vector2d relvel;
        private Body body;

        private void Start()
        {
            body = GetComponent<Body>();
        }

        private void FixedUpdate()
        {
            body.bodyData.KeplerianToCartesian(out relpos, out relvel);
        }
    }
}
