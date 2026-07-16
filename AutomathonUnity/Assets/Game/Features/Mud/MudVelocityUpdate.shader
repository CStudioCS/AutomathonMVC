// Velocity field for the mud "fluid": RG = world-space velocity (units/frame).
// Each frame it diffuses (smooths), damps toward zero, and receives injections:
//   - tanks push directionally along their motion,
//   - explosions push radially outward from the blast centre.
// The colour buffers are advected by sampling this field.
Shader "Hidden/Automathon/MudVelocityUpdate"
{
    Properties { _MainTex ("Velocity", 2D) = "black" {} }
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

            #define MAX_PUSH 8
            #define MAX_BLAST 8

            sampler2D _MainTex;
            float4    _MainTex_TexelSize;
            float     _VelDamping;
            float     _VelDiffuse;   // 0..1 blur mix per frame
            float     _VelBlurSize;
            float2    _ArenaSize;

            int    _PushCount;
            float4 _PusherPos[MAX_PUSH];  // xy = uv, z = radius (world)
            float4 _PusherVel[MAX_PUSH];  // xy = world velocity

            int    _BlastCount;
            float4 _BlastPos[MAX_BLAST];  // xy = uv, z = radius (world), w = outward speed (world)

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
                float2 o = _VelBlurSize * _MainTex_TexelSize.xy;

                float2 v = tex2D(_MainTex, i.uv).rg;
                float2 vb = v;
                vb += tex2D(_MainTex, i.uv + float2(o.x, 0)).rg;
                vb += tex2D(_MainTex, i.uv - float2(o.x, 0)).rg;
                vb += tex2D(_MainTex, i.uv + float2(0, o.y)).rg;
                vb += tex2D(_MainTex, i.uv - float2(0, o.y)).rg;
                vb *= 0.2;
                v = lerp(v, vb, _VelDiffuse) * _VelDamping;

                // Tank injection (directional).
                [loop]
                for (int k = 0; k < _PushCount; k++)
                {
                    float2 wd = (i.uv - _PusherPos[k].xy) * _ArenaSize;
                    float fall = saturate(1.0 - length(wd) / max(_PusherPos[k].z, 1e-4));
                    v += _PusherVel[k].xy * fall;
                }

                // Explosion injection (radial outward from centre).
                [loop]
                for (int bidx = 0; bidx < _BlastCount; bidx++)
                {
                    float2 wd = (i.uv - _BlastPos[bidx].xy) * _ArenaSize;
                    float dist = length(wd);
                    float2 dir = (dist > 1e-4) ? wd / dist : float2(0, 0);
                    float fall = saturate(1.0 - dist / max(_BlastPos[bidx].z, 1e-4));
                    v += dir * _BlastPos[bidx].w * fall;
                }

                return float4(v, 0, 0);
            }
            ENDCG
        }
    }
}
