using UnityEngine;

namespace Decel
{
    public class Pointers : MonoBehaviour
    {
        [SerializeField] private Transform forwardArrow;
        [SerializeField] private Transform progradeArrow;
        [SerializeField] private Transform retrogradeArrow;
        [SerializeField] private GameObject pointer;
        [SerializeField] private float distance;

        private Body body;

        private void Start()
        {
            body = PhysicsManager.mainBody;
        }

        private void Update()
        {
            if (!PredictionManager.Instance || !PredictionManager.Instance.referenceFrameBody) { return; }

            var velocity = (Vector3)(body.bodyData.velocity - PredictionManager.Instance.referenceFrameBody.bodyData.velocity);

            var offset = body.transform.position;
            var position = body.transform.forward * distance + offset;
            forwardArrow.SetPositionAndRotation(position, Quaternion.LookRotation(body.transform.forward));

            if (MathExtentions.FastMagnitude(velocity) < 1f)
            {
                progradeArrow.gameObject.SetActive(false);
                retrogradeArrow.gameObject.SetActive(false);
            }
            else
            {
                progradeArrow.gameObject.SetActive(true);
                retrogradeArrow.gameObject.SetActive(true);

                velocity = velocity.normalized;
                position = velocity * distance + offset;
                progradeArrow.SetPositionAndRotation(position, Quaternion.LookRotation(velocity));
                position = -velocity * distance + offset;
                retrogradeArrow.SetPositionAndRotation(position, Quaternion.LookRotation(velocity));
            }
        }
    }
}
