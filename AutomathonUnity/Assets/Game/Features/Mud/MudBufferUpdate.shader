// Premultiplied colour-accumulation buffer (fluid look): RGB = sum(colour*weight), A = sum(weight).
// Advect by the fluid velocity field, diffuse-blur, fade, then add coloured stamps.
Shader "Hidden/Automathon/MudBufferUpdate"
{
    Properties { _MainTex ("Buffer", 2D) = "black" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            #define MAX_CIRCLE 128

            sampler2D _MainTex;
            float4    _MainTex_TexelSize;
            sampler2D _VelBuffer;   // RG = world velocity (the fluid field)
            float     _AdvectScale;
            float     _Fade;
            float     _BlurSize;
            float     _BlurRate;
            float2    _ArenaSize;

            int    _CircleCount;
            float4 _CirclePos[MAX_CIRCLE];   // xy = uv, z = radius (world), w = strength
            float4 _CircleColor[MAX_CIRCLE]; // rgb

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 o = _BlurSize * _MainTex_TexelSize.xy;

                // Advection along the shared velocity field (the fluid).
                float2 velWorld = tex2D(_VelBuffer, i.uv).rg;
                float2 suv = i.uv - (velWorld / _ArenaSize) * _AdvectScale;

                // Blur partway toward the 5-tap average, then fade.
                float4 c = tex2D(_MainTex, suv);
                float4 b = c;
                b += tex2D(_MainTex, suv + float2(o.x, 0));
                b += tex2D(_MainTex, suv - float2(o.x, 0));
                b += tex2D(_MainTex, suv + float2(0, o.y));
                b += tex2D(_MainTex, suv - float2(0, o.y));
                b *= 0.2;
                float4 v = lerp(c, b, _BlurRate) * _Fade;

                // Circular marks (premultiplied colour).
                [loop]
                for (int a = 0; a < _CircleCount; a++)
                {
                    float2 wd = (i.uv - _CirclePos[a].xy) * _ArenaSize;
                    float d = length(wd);
                    float f = saturate(1.0 - d / max(_CirclePos[a].z, 1e-4)) * _CirclePos[a].w;
                    v.rgb += _CircleColor[a].rgb * f;
                    v.a += f;
                }

                return v;
            }
            ENDCG
        }
    }
}
