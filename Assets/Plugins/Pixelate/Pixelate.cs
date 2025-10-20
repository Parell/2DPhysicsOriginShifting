using UnityEngine;

//https://github.com/tasdidahmedtah/Unity_PostEffect/tree/master
public class Pixelate : MonoBehaviour
{
	[Range(1, 1920)]
	public int horizontal = 20;
	[Range(1, 1080)]
	public int vertical = 20;
	public Material material;

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(src, dest, material);
	}

	private void Update()
	{
		material.SetInt("_Horizontal", horizontal);
		material.SetInt("_Vertical", vertical);
	}
}