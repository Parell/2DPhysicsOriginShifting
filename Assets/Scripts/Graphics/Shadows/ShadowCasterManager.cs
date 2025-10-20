using System.Collections.Generic;
using UnityEngine;

namespace Decel
{
    [DefaultExecutionOrder(-10)]
    [ExecuteAlways]
    public class ShadowCasterManager : MonoBehaviour
    {
        public static ShadowCasterManager Instance;
        [SerializeField] private Transform starScaledTransform;
        [SerializeField] private List<ShadowCaster> shadowCasters;
        [SerializeField] private float angularDiameterDeg = 6;

        private const int MAX_SHADOWS = 4;

        [SerializeField] private RenderTexture[] renderTextures;
        private Camera silhouetteCamera;


        private void OnEnable()
        {
            Instance = this;
            renderTextures = new RenderTexture[MAX_SHADOWS];
            CreateSilhouetteCamera();
        }

        private void OnDisable()
        {
            if (silhouetteCamera != null) { DestroyImmediate(silhouetteCamera.gameObject); }
            for (int i = 0; i < renderTextures.Length; i++)
            {
                if (renderTextures[i] != null)
                {
                    renderTextures[i].Release();
                    DestroyImmediate(renderTextures[i]);
                    renderTextures[i] = null;
                }
            }
        }

        private void CreateSilhouetteCamera()
        {
            if (silhouetteCamera != null) { return; }
            GameObject cameraObject = new GameObject("SilhouetteCamera", typeof(Camera));
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            silhouetteCamera = cameraObject.GetComponent<Camera>();
            silhouetteCamera.enabled = false;
            silhouetteCamera.orthographic = true;
            silhouetteCamera.clearFlags = CameraClearFlags.SolidColor;
            silhouetteCamera.backgroundColor = Color.white;
            silhouetteCamera.allowHDR = false;
            silhouetteCamera.allowMSAA = false;
            silhouetteCamera.allowDynamicResolution = false;
            silhouetteCamera.useOcclusionCulling = false;
            silhouetteCamera.renderingPath = RenderingPath.Forward;
            silhouetteCamera.cullingMask = 0;
            silhouetteCamera.orthographicSize = 1;
            silhouetteCamera.nearClipPlane = 0.001f;
            silhouetteCamera.farClipPlane = 100;
        }

        public void AddShadowCaster(ShadowCaster shadowCaster)
        {
            if (shadowCasters.Contains(shadowCaster)) { return; }
            shadowCasters.Add(shadowCaster);
        }

        public void RemoveShadowCaster(ShadowCaster shadowCaster)
        {
            shadowCasters.Remove(shadowCaster);
        }

        private void Update()
        {
            if (starScaledTransform == null || silhouetteCamera == null) { return; }

            int count = Mathf.Min(shadowCasters.Count, Mathf.Min(4, MAX_SHADOWS));

            for (int i = 0; i < count; i++)
            {
                var shadowCaster = shadowCasters[i];
                if (shadowCaster.scaledMesh == null)
                {
                    Debug.Log("Shadow caster has no assigned mesh, gameobject " + shadowCaster.gameObject.name);
                    shadowCasters.Remove(shadowCaster);
                    continue;
                }

                if (renderTextures[i] == null)
                {
                    int size = shadowCaster.shadowTextureSize;
                    renderTextures[i] = new RenderTexture(size, size, 0, RenderTextureFormat.R8);
                    renderTextures[i].wrapMode = TextureWrapMode.Clamp;
                    renderTextures[i].filterMode = FilterMode.Point;
                    renderTextures[i].name = shadowCaster.gameObject.name;
                }

                RenderSilhouetteToRenderTexture(shadowCaster, renderTextures[i]);

                Shader.SetGlobalTexture("_ShadowTexture" + i, renderTextures[i]);
                Matrix4x4 projection = GL.GetGPUProjectionMatrix(silhouetteCamera.projectionMatrix, false) * silhouetteCamera.worldToCameraMatrix;
                Shader.SetGlobalMatrix("_WorldToShadow" + i, projection);
                Shader.SetGlobalVector("_ShadowPosition" + i, shadowCaster.scaledTransform.position);
                Shader.SetGlobalFloat("_ShadowRadius" + i, shadowCaster.radius * shadowCaster.scaledTransform.lossyScale.x);

                float orthoHalf = silhouetteCamera.orthographicSize;           // vertical half-size used for that body
                float aspect = (float)renderTextures[i].width / renderTextures[i].height;       // 1 if square
                Shader.SetGlobalFloat("_ShadowOrthoHalf" + i, orthoHalf);
                Shader.SetGlobalFloat("_ShadowAspect" + i, aspect);
                Shader.SetGlobalFloat("_ShadowTexWidth" + i, renderTextures[i].width);
            }

            for (int i = count; i < MAX_SHADOWS; i++)
            {
                Shader.SetGlobalTexture("_ShadowTexture" + i, Texture2D.whiteTexture);
                Shader.SetGlobalMatrix("_WorldToShadow" + i, Matrix4x4.identity);
                Shader.SetGlobalVector("_ShadowPosition" + i, Vector4.zero);
                Shader.SetGlobalFloat("_ShadowRadius" + i, 0);

                Shader.SetGlobalFloat("_ShadowOrthoHalf" + i, 0);
                Shader.SetGlobalFloat("_ShadowAspect" + i, 0);
                Shader.SetGlobalFloat("_ShadowTexWidth" + i, 0);
            }

            Shader.SetGlobalInt("_ShadowCount", count);
            Shader.SetGlobalFloat("_StarHalfAngleTan", Mathf.Tan(0.5f * angularDiameterDeg * Mathf.Deg2Rad));
        }

        private void RenderSilhouetteToRenderTexture(ShadowCaster shadowCaster, RenderTexture renderTexture)
        {
            Vector3 starDirection = starScaledTransform.forward;
            Vector3 position = shadowCaster.scaledTransform.position;
            float size = shadowCaster.radius * shadowCaster.scaledTransform.lossyScale.x * 1.1f;

            silhouetteCamera.transform.SetPositionAndRotation(position - (starDirection * size), Quaternion.LookRotation(starDirection, Vector3.up));
            int layer = shadowCaster.scaledTransform.gameObject.layer;
            silhouetteCamera.cullingMask = 1 << layer;
            silhouetteCamera.targetTexture = renderTexture;

            float renderTextureAspect = (float)renderTexture.width / renderTexture.height;
            silhouetteCamera.ResetProjectionMatrix();
            silhouetteCamera.projectionMatrix = Matrix4x4.Ortho(-size * renderTextureAspect, size * renderTextureAspect, -size, size, 0.01f, size * 2);
            silhouetteCamera.Render();

            var material = shadowCaster.shadowMaterial;
            if (material == null) { material = new Material(Shader.Find("Hidden/Shadow")); }

            Graphics.DrawMesh(shadowCaster.scaledMesh, shadowCaster.scaledTransform.localToWorldMatrix, material, layer, silhouetteCamera, 0);

            silhouetteCamera.targetTexture = null;
        }
    }
}
