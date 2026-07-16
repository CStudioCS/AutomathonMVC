// Additive unlit shader for the ambient glowing pixel-particles.
// Output = _Color * per-vertex brightness, added to the frame (bloom turns it into glow).
Shader "Automathon/AmbientParticle"
{
    Properties { _Color ("Color", Color) = (1.3, 1.35, 1.5, 1) }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend One One          // additive
        ZWrite Off
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; };
            struct Varyings   { float4 positionCS : SV_POSITION; float4 color : COLOR; };

            float4 _Color;

            Varyings vert (Attributes IN)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.color = IN.color;
                return o;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                return half4(_Color.rgb * IN.color.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}
