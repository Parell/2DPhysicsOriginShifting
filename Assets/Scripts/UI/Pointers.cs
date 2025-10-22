using UnityEngine;

namespace Decel
{
    public class Pointers : MonoBehaviour
    {
        [SerializeField] private Transform forwardArrow;
        [SerializeField] private Transform progradeArrow;
        [SerializeField] private Transform retrogradeArrow;
        [SerializeField] private float distance;

        private Body body;

        private void Start()
        {
            body = PhysicsManager.mainBody;
        }

        private void Update()
        {
            if (!PredictionManager.Instance || !body.bodyData.attractor) { return; }

            var velocity = body.bodyData.velocity - body.bodyData.attractor.bodyData.velocity;

            Vector2 offset = body.transform.position;
            var position = (Vector2)body.transform.up * distance + offset;
            forwardArrow.SetPositionAndRotation(position, Quaternion.LookRotation(body.transform.up));

            if (velocity.magnitude < 1f)
            {
                progradeArrow.gameObject.SetActive(false);
                retrogradeArrow.gameObject.SetActive(false);
            }
            else
            {
                progradeArrow.gameObject.SetActive(true);
                retrogradeArrow.gameObject.SetActive(true);

                velocity = velocity.normalized;
                position = (Vector2)velocity * distance + offset;
                progradeArrow.SetPositionAndRotation(position, Quaternion.LookRotation((Vector2)velocity));
                position = -(Vector2)velocity * distance + offset;
                retrogradeArrow.SetPositionAndRotation(position, Quaternion.LookRotation((Vector2)velocity));
            }
        }
    }
}
