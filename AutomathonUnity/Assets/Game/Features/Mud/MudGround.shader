// Smooth (fluid) compositing of three premultiplied colour buffers over the base mud.
//   Slow: trail + explosion | Fast: shield | Push: bullet + missile.
Shader "Automathon/MudGround"
{
    Properties
    {
        _MudBufferSlow ("Slow Buffer", 2D) = "black" {}
        _MudBufferFast ("Fast Buffer", 2D) = "black" {}
        _MudBufferPush ("Push Buffer", 2D) = "black" {}
        // Mud colours (_Mud0.._Mud3) are plain uniforms set from script (LinRGB), NOT Color
        // properties — Color-typed properties get auto gamma-converted in Linear space.
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MudBufferSlow;
            sampler2D _MudBufferFast;
            sampler2D _MudBufferPush;
            // Mud base ramps through 4 neon-city blues by the noise value.
            float4 _Mud0;   // darkest
            float4 _Mud1;
            float4 _Mud2;
            float4 _Mud3;   // brightest (also the trail colour -> tank trace lightens the mud)
            float  _StainWarp;     // domain-warp amount (organic swirl)
            float2 _NoiseScale;    // arena UV -> noise cells (keeps blotches isotropic)

            sampler2D _WarpBuffer; // RG = accumulated world displacement from the fluid
            float2 _ArenaSize;
            float  _WarpDisplace;  // how far the fluid drags the stain texture

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 Composite (float3 baseCol, float4 m)
            {
                float3 markCol = (m.a > 1e-4) ? m.rgb / m.a : baseCol;
                return lerp(baseCol, markCol, saturate(m.a));
            }

            // --- Value noise + fbm, domain-warped into flowing ink-stain shapes ---
            float hash21 (float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            float vnoise (float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);   // smooth interp -> no blocky grid
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }
            float fbm (float2 p)
            {
                float s = 0.0, a = 0.5;
                [unroll]
                for (int k = 0; k < 4; k++) { s += a * vnoise(p); p *= 2.0; a *= 0.5; }
                return s;
            }
            // Feed fbm through fbm: the warp bends the field so patches look brushed/scribbled
            // rather than like uniform static.
            float StainNoise (float2 uv)
            {
                float2 p = uv * _NoiseScale;
                float2 q = float2(fbm(p), fbm(p + float2(5.2, 1.3)));
                float n = fbm(p + _StainWarp * q);
                return smoothstep(0.25, 0.75, n);      // soft blotches
            }

            // Ramp the noise across the 4 neon-city mud blues (dark -> bright).
            float3 MudRamp (float t)
            {
                t = saturate(t) * 3.0;
                float3 a = lerp(_Mud0.rgb, _Mud1.rgb, saturate(t));
                float3 b = lerp(_Mud1.rgb, _Mud2.rgb, saturate(t - 1.0));
                float3 c = lerp(_Mud2.rgb, _Mud3.rgb, saturate(t - 2.0));
                return (t < 1.0) ? a : ((t < 2.0) ? b : c);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Drag the stain lookup by the accumulated fluid displacement so trails/blasts
                // push the texture around (and it stays pushed until the field relaxes).
                float2 warp = tex2D(_WarpBuffer, i.uv).rg;
                float2 stainUv = i.uv - (warp / _ArenaSize) * _WarpDisplace;
                // Base mud: noise ramps the darker blues; the trail buffer lightens toward the brightest.
                float3 col = MudRamp(StainNoise(stainUv) * 0.8);
                col = Composite(col, tex2D(_MudBufferSlow, i.uv));
                col = Composite(col, tex2D(_MudBufferPush, i.uv));
                col = Composite(col, tex2D(_MudBufferFast, i.uv));
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
