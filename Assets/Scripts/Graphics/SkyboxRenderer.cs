using UnityEngine;

namespace Decel
{
    public class SkyboxRenderer : MonoBehaviour
    {
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private Material material;

        private void LateUpdate()
        {
            UpdateCubemap(63);
        }

        private void UpdateCubemap(int faceMask)
        {
            GameObject newGameObject = new GameObject("CubemapCamera");
            newGameObject.AddComponent<Camera>();
            newGameObject.hideFlags = HideFlags.HideAndDontSave;
            newGameObject.transform.position = transform.position;
            newGameObject.transform.rotation = Quaternion.identity;
            Camera camera = newGameObject.GetComponent<Camera>();
            camera.farClipPlane = 100;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.enabled = false;

            renderTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            renderTexture.hideFlags = HideFlags.HideAndDontSave;

            material.SetTexture("_Tex", renderTexture);

            camera.transform.position = transform.position;
            camera.RenderToCubemap(renderTexture, faceMask);
            Destroy(camera);
            Destroy(gameObject);
        }
    }
}