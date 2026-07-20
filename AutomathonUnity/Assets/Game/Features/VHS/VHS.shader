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

        [Header(Glitch)]
        _GlitchIntensity   ("Glitch Intensity (driven) - scales line density", Range(0,2)) = 0
        _GlitchLineOffset  ("Glitch Line Offset (uv, driven) - shift distance", Range(0,0.2)) = 0
        _GlitchSpeed       ("Glitch Wobble Speed", Float) = 60
        _GlitchLineBands   ("Glitch Line Bands", Float) = 64
        _GlitchLineDensity ("Glitch Line Density", Range(0,1)) = 0.35
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
            float  _GlitchIntensity;
            float  _GlitchLineOffset;
            float  _GlitchSpeed;
            float  _GlitchLineBands;
            float  _GlitchLineDensity;

            half4 Frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                // --- Pixelation to _PixelHeight lines of blocks (aspect-correct) ---
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);
                float2 grid = float2(_PixelHeight * aspect, _PixelHeight);
                float2 puv = (floor(uv * grid) + 0.5) / grid;

                // --- Glitch: shove certain horizontal line-bands sideways with a rapid sine ---
                float gx = 0.0;
                if (_GlitchIntensity > 0.0 || _GlitchLineOffset > 0.0)
                {
                    float band = floor(uv.y * _GlitchLineBands);
                    // per-band gate that reshuffles a few times a second -> only some bands jump
                    float gate = frac(sin(band * 78.233 + floor(_Time.y * 12.0) * 3.71) * 43758.5453);
                    // intensity scales how MANY bands jump (density); offset is the shift distance
                    float density = _GlitchLineDensity * saturate(_GlitchIntensity);
                    float sel = step(1.0 - density, gate);
                    float wave = sin(_Time.y * _GlitchSpeed + band * 1.7);   // rapid horizontal wobble
                    gx = sel * wave * _GlitchLineOffset;
                }

                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, puv + float2(gx, 0.0)).rgb;

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
