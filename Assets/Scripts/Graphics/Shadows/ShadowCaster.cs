using UnityEngine;

namespace Decel
{
    [ExecuteAlways]
    public class ShadowCaster : MonoBehaviour
    {
        public Mesh scaledMesh;
        public Material shadowMaterial;
        public Transform scaledTransform;
        public int shadowTextureSize = 512;
        public float radius = 1;
        private Body body;

        // Wait till mesh created and the find bounds or get bounds from body, do later.

        private void OnEnable()
        {
            body = GetComponent<Body>();
            //scaledTransform = body.scaledTransform;

            //var scaledMeshScript = GetComponent<ScaledMesh>();

            //if (scaledMeshScript != null)
            //{
            //    scaledMesh = scaledMeshScript.scaledMesh;
            //}

            if (shadowMaterial == null)
            {
                shadowMaterial = Resources.Load<Material>("Materials/Shadow");
                if (shadowMaterial == null)
                {
                    Debug.Log("Shadow material path has moved");
                }
            }

            ShadowCasterManager.Instance.AddShadowCaster(this);
        }

        private void OnDisable()
        {
            ShadowCasterManager.Instance.RemoveShadowCaster(this);
        }
    }
}
