// Full-screen post effect for the URP Full Screen Pass Renderer Feature.
// Pared down to just: 360p pixelation + gray semi-transparent horizontal lines.
// (Barrel, chromatic aberration, grain, bloom, vignette are handled by built-in URP effects.)
Shader "Automathon/LightExplosion"
{
    Properties
    {
        _ExplosionEpicenterX ("Explosion Epicenter X - screen coordinates", Range(0,1)) = 0.5    
        _ExplosionEpicenterY ("Explosion Epicenter Y - screen coordinates", Range(0,1)) = 0.5
        _ExplosionGlobalAdvancement ("Explosion Global Advancement - 0 to 1", Float) = 0
        _AdvancementForFullScreen ("Advancement for Full Screen", Float) = 0.8
        _BrigthnessIncreaseSpeed ("Brightness Increase Speed", Float) = 5 // nb of luminosity units per percentage of advancement (cf HSL colors)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off
        Pass
        {
            Name "LightExplosion"
            ZTest Always

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _ExplosionEpicenterX;        
            float _ExplosionEpicenterY;
            float _ExplosionGlobalAdvancement;  
            float _AdvancementForFullScreen;
            float _BrigthnessIncreaseSpeed;  

            float3 RGBtoHSL(float3 c)
            {
                float maxc = max(c.r, max(c.g, c.b));
                float minc = min(c.r, min(c.g, c.b));
                float delta = maxc - minc;

                float L = (maxc + minc) * 0.5;

                float H = 0.0;
                float S = 0.0;

                if (delta > 1e-6)
                {
                    // Saturation
                    S = delta / (1.0 - abs(2.0 * L - 1.0));

                    // Hue
                    if (maxc == c.r)
                        H = fmod((c.g - c.b) / delta, 6.0);
                    else if (maxc == c.g)
                        H = (c.b - c.r) / delta + 2.0;
                    else
                        H = (c.r - c.g) / delta + 4.0;

                    H = H / 6.0;
                    if (H < 0.0) H += 1.0;
                }

                return float3(H, S, L); // all in [0,1]
            }
            
            float3 HueToRGB(float p, float q, float t)
            {
                if (t < 0.0) t += 1.0;
                if (t > 1.0) t -= 1.0;
                if (t < 1.0/6.0) return p + (q - p) * 6.0 * t;
                if (t < 1.0/2.0) return q;
                if (t < 2.0/3.0) return p + (q - p) * (2.0/3.0 - t) * 6.0;
                return p;
            }

            float3 HSLtoRGB(float3 hsl)
            {
                float H = hsl.x;
                float S = hsl.y;
                float L = hsl.z;

                if (S <= 1e-6)
                    return float3(L, L, L); // achromatic

                float q = (L < 0.5) ? L * (1.0 + S) : L + S - L * S;
                float p = 2.0 * L - q;

                float r = HueToRGB(p, q, H + 1.0/3.0);
                float g = HueToRGB(p, q, H);
                float b = HueToRGB(p, q, H - 1.0/3.0);

                return float3(r, g, b);
            }

            half4 Frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

                // Compute furthest distance from Epicenter
                float distCorner1 = distance(float2(0, 0), float2(_ExplosionEpicenterX, _ExplosionEpicenterY));
                float distCorner2 = distance(float2(0, 1), float2(_ExplosionEpicenterX, _ExplosionEpicenterY));
                float distCorner3 = distance(float2(1, 0), float2(_ExplosionEpicenterX, _ExplosionEpicenterY));
                float distCorner4 = distance(float2(1, 1), float2(_ExplosionEpicenterX, _ExplosionEpicenterY));

                float furthest = max(max(distCorner1, distCorner2), max(distCorner3, distCorner4));

                //this being reached at _AdvancementForFullScreen advancement, compute the propagation speed (linear) and knowing the actual global advancement, compute the explosion range and local advancement
                float propagationSpeed = furthest/_AdvancementForFullScreen;
                float explosionRange = _ExplosionGlobalAdvancement * propagationSpeed;
                float localRange = distance(float2(_ExplosionEpicenterX, _ExplosionEpicenterY), uv);
                if (localRange > explosionRange)
                {
                    return half4(col, 1);
                }
                float localAdvancement = (explosionRange - localRange) / propagationSpeed;

                //compute actual luminosity of my pixel
                float3 hsl = RGBtoHSL(col);
                float luminosity = hsl.z;

                //deduce the brightness for the actual pixel, capped at 100
                float brightness = saturate(luminosity + localAdvancement * _BrigthnessIncreaseSpeed);
                
                //final colour
                col = HSLtoRGB(float3(hsl.x, hsl.y, brightness));
                

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
