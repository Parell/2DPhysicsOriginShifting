using UnityEngine;

namespace Decel
{
    public class Rotate : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationSpeed;

        private void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}
