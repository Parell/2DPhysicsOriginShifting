using UnityEngine;

namespace Decel
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private bool cameraUp = true;
        [SerializeField] private bool scaleByDistance = false;
        [SerializeField] private Vector2 scaleRange;
        private Transform cameraTransfrom;
        [SerializeField] private float maxScaleDistance;

        private void Start()
        {
            cameraTransfrom = Camera.main.transform;
        }

        private void LateUpdate()
        {
            Vector3 forward = cameraTransfrom.position - transform.position;

            if (scaleByDistance)
            {
                transform.localScale = Vector3.one * Mathf.Lerp(scaleRange.x, scaleRange.y, Mathf.Clamp01(MathExtentions.FastMagnitude(forward) / maxScaleDistance));
            }


            if (cameraUp)
            {
                transform.rotation = Quaternion.LookRotation(forward, cameraTransfrom.up);
                return;
            }
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}
