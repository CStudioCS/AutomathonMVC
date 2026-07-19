// Accumulated displacement field for the base "stain" texture.
// RG = world-space displacement of the ground material at each texel. Each frame it rides
// the shared velocity field (advection), accumulates the current flow, and slowly relaxes,
// so tank trails / blasts drag the texture around and it stays dragged for a while.
Shader "Hidden/Automathon/MudWarpUpdate"
{
    Properties { _MainTex ("Warp", 2D) = "black" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _VelBuffer;   // RG = world velocity (the fluid field)
            float2    _ArenaSize;
            float     _WarpAdvect;  // how strongly the displacement rides the flow
            float     _WarpGain;    // velocity accumulated into displacement per frame
            float     _WarpDamp;    // per-frame relax back toward zero
            float     _WarpMax;     // clamp on displacement magnitude (world units)

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            float4 frag (v2f i) : SV_Target
            {
                float2 vel = tex2D(_VelBuffer, i.uv).rg;
                float2 suv = i.uv - (vel / _ArenaSize) * _WarpAdvect;
                float2 d = tex2D(_MainTex, suv).rg;   // advect prior displacement along the flow
                d += vel * _WarpGain;                  // accumulate current flow
                d *= _WarpDamp;                        // slowly settle back
                d /= max(1.0, length(d) / _WarpMax);   // clamp runaway
                return float4(d, 0, 0);
            }
            ENDCG
        }
    }
}
