//
// KinoFringe - Chromatic aberration effect
//
// Copyright (C) 2015 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
Shader "Hidden/Kino/Fringe"
{
    Properties
    {
        _MainTex ("-", 2D) = "" {}
    }

    CGINCLUDE

    #pragma multi_compile

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    float4 _CameraAspect; // (h/w, w/h, 1, 0)
    float _LateralShift;
    float _AxialStrength;
    float _AxialShift;

    // Poisson disk sample points
    static const uint SAMPLE_NUM = 8;
    static const float2 POISSON_SAMPLES[SAMPLE_NUM] =
    {
        float2( 0.373838022357f, 0.662882019975f ),
        float2( -0.335774814282f, -0.940070127794f ),
        float2( -0.9115721822f, 0.324130702404f ),
        float2( 0.837294074715f, -0.504677167232f ),
        float2( -0.0500874221246f, -0.0917990757772f ),
        float2( -0.358644570242f, 0.906381100284f ),
        float2( 0.961200130218f, 0.219135111748f ),
        float2( -0.896666615007f, -0.440304757692f )
    };

    // Poisson filter
    half3 poisson_filter(float2 uv)
    {
        half3 acc = 0;
        for (uint i = 0; i < SAMPLE_NUM; i++)
        {
            float2 disp = POISSON_SAMPLES[i];
            disp *= _CameraAspect.yz * _AxialShift * 0.02;
            acc += tex2D(_MainTex, uv + disp).rgb;
        }
        return acc / SAMPLE_NUM;
    }

    // Rec.709 Luminance
    half luminance(half3 rgb)
    {
        return dot(rgb, half3(0.2126, 0.7152, 0.0722));
    }

    // CA filter
    half4 frag(v2f_img i) : SV_Target
    {
        float2 spc = (i.uv - 0.5) * _CameraAspect.xz;
        float r2 = dot(spc, spc);

        float f_r = 1.0 + r2 * _LateralShift * -0.02;
        float f_b = 1.0 + r2 * _LateralShift * +0.02;

        half4 src = tex2D(_MainTex, i.uv);
        src.r = tex2D(_MainTex, (i.uv - 0.5) * f_r + 0.5).r;
        src.b = tex2D(_MainTex, (i.uv - 0.5) * f_b + 0.5).b;

        half3 blur = poisson_filter(i.uv);
        half ldiff = luminance(blur) - luminance(src.rgb);
        src.rb = max(src.rb, blur.rb * ldiff * _AxialStrength);

        return src;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}
