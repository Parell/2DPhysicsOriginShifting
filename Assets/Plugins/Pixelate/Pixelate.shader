Shader "Custom/Mosaic" {
	// https://docs.unity3d.com/ja/540/Manual/SL-Properties.html
	Properties {
        _MainTex ("Texture", 2D) = ""{}
		_Horizontal ("Horizontal", int) = 20
		_Vertical ("Vertical", int) = 20
	}


	SubShader {
		Pass {
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag

			// https://docs.unity3d.com/ja/540/Manual/SL-UnityShaderVariables.html
			sampler2D _MainTex;
			int _Horizontal;
			int _Vertical;

			half4 frag(v2f_img img): COLOR 
			{
				float centerH;
				if(_Horizontal == 1)
				{
					centerH = 0.5;
				}
				else
				{
					centerH = 1.0 / float(_Horizontal) * 0.5;
				}

				float centerV;
				if(_Vertical == 1)
				{
					centerV = 0.5;
				}
				else
				{
					centerV = 1.0 / float(_Vertical) * 0.5;
				}

				img.uv.x = floor(img.uv.x * _Horizontal) / _Horizontal + centerH;
				img.uv.y = floor(img.uv.y * _Vertical) / _Vertical + centerV;

				return tex2D(_MainTex, img.uv);
			}
			ENDCG
		}
	}

	// https://docs.unity3d.com/jp/530/Manual/SL-Fallback.html
	FallBack "Diffuse"
}
