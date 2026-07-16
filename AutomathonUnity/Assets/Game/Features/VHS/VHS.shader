// Full-screen post effect for the URP Full Screen Pass Renderer Feature.
// Pared down to just: 360p pixelation + gray semi-transparent horizontal lines.
// (Barrel, chromatic aberration, grain, bloom, vignette are handled by built-in URP effects.)
Shader "Automathon/VHS"
{
    Properties
    {
        _PixelHeight ("Vertical Resolution (px)", Float) = 360
        _LineCount   ("Horizontal Line Count", Float) = 240
        _LineOpacity ("Horizontal Line Opacity", Range(0,1)) = 0.07
        _LineColor   ("Horizontal Line Color", Color) = (0.6, 0.6, 0.65, 1)
        _ScanCount    ("Scanline Count", Float) = 90
        _ScanStrength ("Scanline Strength", Range(0,1)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off
        Pass
        {
            Name "VHS"
            ZTest Always

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float  _PixelHeight;
            float  _LineCount;
            float  _LineOpacity;
            float4 _LineColor;
            float  _ScanCount;
            float  _ScanStrength;

            half4 Frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                // --- Pixelation to _PixelHeight lines of blocks (aspect-correct) ---
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);
                float2 grid = float2(_PixelHeight * aspect, _PixelHeight);
                float2 puv = (floor(uv * grid) + 0.5) / grid;

                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, puv).rgb;

                // --- Horizontal gray lines (soft, semi-transparent overlay) ---
                float cell = frac(uv.y * _LineCount) - 0.5;   // centred -0.5..0.5 within each line period
                float lineBand = 1.0 - smoothstep(0.15, 0.30, abs(cell));
                col = lerp(col, _LineColor.rgb, lineBand * _LineOpacity);

                // --- Smooth sine scanlines across the whole screen (the overall brightness wave) ---
                float scan = 0.5 + 0.5 * sin(uv.y * _ScanCount * 6.2831853);
                col *= 1.0 - _ScanStrength * scan;

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
