// Two-tone sprite recolour: remaps the sprite's luminance to a dark->light colour ramp,
// so the base (green) tank skin can be tinted to any two colours. Keeps the sprite's alpha
// (shape) and the SpriteRenderer's alpha (used for the dash fade). Unlit.
Shader "Automathon/TankTwoTone"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _LightColor ("Light Color", Color) = (1, 1, 1, 1)
        _DarkColor  ("Dark Color",  Color) = (0, 0, 0, 1)
        _Threshold  ("Mid Threshold", Range(0,1)) = 0.5
        _Softness   ("Softness", Range(0.001,1)) = 0.4
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings   { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _LightColor;
            float4 _DarkColor;
            float  _Threshold;
            float  _Softness;

            Varyings vert (Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.uv = i.uv;
                o.color = i.color;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float lum = dot(tex.rgb, float3(0.299, 0.587, 0.114));
                float t = smoothstep(_Threshold - _Softness, _Threshold + _Softness, lum);
                float3 col = lerp(_DarkColor.rgb, _LightColor.rgb, t);
                return half4(col, tex.a * i.color.a);   // shape from sprite alpha, fade from SpriteRenderer alpha
            }
            ENDHLSL
        }
    }
}
