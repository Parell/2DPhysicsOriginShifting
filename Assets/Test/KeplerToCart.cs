using UnityEngine;

namespace Decel
{
    public class KeplerToCart : MonoBehaviour
    {
        private Body body;
        [SerializeField] private Vector2d position;
        [SerializeField] private Vector2d velocity;

        private void Start()
        {
            body = GetComponent<Body>();
        }

        private void FixedUpdate()
        {
            (position, velocity) = body.bodyData.KeplerianToCartesian(Time.fixedDeltaTime);
        }
    }
}
