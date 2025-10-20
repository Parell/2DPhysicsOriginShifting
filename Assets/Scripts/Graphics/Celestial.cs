using UnityEngine;

namespace Decel
{
    public class Celestial : MonoBehaviour
    {
        // 3000m planet 800m moon
        [SerializeField] private float surfaceGravity = Constant.G0;
        public float radius = 1;
        [SerializeField] private Mesh localMesh;
        [SerializeField] private Material material;
        [SerializeField] private GameObject surface;
        private bool inEditor => Application.isEditor && !Application.isPlaying;
        private Body body;

        private void OnValidate()
        {
            if (inEditor)
            {
                body = GetComponent<Body>();
                body.bodyData.mass = MassOfSphere(surfaceGravity, radius);
            }
        }

        private double MassOfSphere(float surfaceGravity, float radius)
        {
            return surfaceGravity * (radius * radius) / Constant.G;
        }

        [ContextMenu("Create Surface")]
        private void CreateSurface()
        {
            DeleteSurface();
            if (surface == null)
            {
                surface = new GameObject("Surface");
                surface.layer = 0;
                var localTransform = surface.transform;
                localTransform.parent = transform;
                localTransform.localScale = Vector3.one * radius;
                localTransform.localPosition = Vector3.zero;

                var meshFilter = surface.AddComponent<MeshFilter>();
                var meshRenderer = surface.AddComponent<MeshRenderer>();
                var meshCollider = surface.AddComponent<MeshCollider>();

                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
                meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

                meshFilter.sharedMesh = localMesh;
                meshRenderer.sharedMaterial = material;
                meshCollider.sharedMesh = localMesh;
            }
        }

        [ContextMenu("Delete Surface")]
        private void DeleteSurface()
        {
            if (surface != null)
            {
                DestroyImmediate(surface);
            }
        }
    }
}
