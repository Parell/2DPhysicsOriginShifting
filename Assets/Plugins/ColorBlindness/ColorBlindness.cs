using UnityEngine;

public class ColorBlindness : MonoBehaviour
{
    public Shader colorBlindnessShader;
    public enum BlindTypes { Protanomaly = 0, Deuteranomaly, Tritanomaly }
    public BlindTypes blindType;
    [Range(0, 1)]
    public float severity = 0;
    public bool difference = false;
    private Material _material;

    private void Start()
    {
        _material ??= new Material(colorBlindnessShader);
        _material.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _material.SetFloat("_Severity", severity);
        _material.SetInt("_Difference", difference ? 1 : 0);

        Graphics.Blit(source, destination, _material, (int)blindType);
    }
}
