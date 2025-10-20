using UnityEngine;

namespace Decel
{
    public class DeallocateParticleSystem : MonoBehaviour
    {
        private Body body;

        private void Start()
        {
            body = GetComponent<Body>();
        }

        private void OnParticleSystemStopped()
        {
            PhysicsManager.Instance.RemoveBody(body);
            PoolManager.Deallocate(gameObject);
        }
    }
}
