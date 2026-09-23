// Full-screen post effect for the URP Full Screen Pass Renderer Feature.
// Pared down to just: 360p pixelation + gray semi-transparent horizontal lines.
// (Barrel, chromatic aberration, grain, bloom, vignette are handled by built-in URP effects.)

Shader "Automathon/LightExplosion"
{
    Properties
    {

        [Header(Global)]
        _ExplosionEpicenterX ("Explosion Epicenter X - screen coordinates", Range(0,1)) = 0.5    
        _ExplosionEpicenterY ("Explosion Epicenter Y - screen coordinates", Range(0,1)) = 0.5

        _ActivateBlast ("Activate Blast - 1 yes, 0 no", int) = 0
        _ActivateRays ("Activate Rays - 1 yes, 0 no", int) = 0
        _ActivateLargeRays ("Activate Large Rays - 1 yes, 0 no", int) = 0

        _BlastAdvancement ("BlastAdvancement", Range(0,1)) = 0
        _RaysAdvancement ("RaysAdvancement", Range(0,1)) = 0
        _LargeRaysAdvancement ("LargeRaysAdvancement", Range(0,1)) = 0

        _GlobalLuminosityDecrease ("Global Luminosity Decrease", float) = 1 // given by light explosion script, depends on the frame's deltaTime, constant decrease speed over time



        [Header(Blast)]
        _ExplosionRange ("Explosion Range - screen distance", Float) = 0.5
        _RingWidth ("Ring Width - screen distance", Float) = 0.02
        _FadingSpeed ("Fading Speed inside Ring - lum units/advancement percentage", Float) = 2

        _AdvancementForFullScreen ("Advancement for Full Screen", Float) = 0.8
        _BrigthnessIncreaseSpeed ("Brightness Increase Speed", Float) = 5 // nb of luminosity units per percentage of advancement (cf HSL colors)

        [Header(Large_Rays)]


        [Header(Rays_General)]
        _RaysSeed ("Rays Seed", Float) = 0
        _RaysCount ("Rays Count - max 12", int) = 0
        _RaysStartingAdvancement ("_RaysStartingAdvancement", Range(0,1)) = 0.2
        _RaysEndingAdvancement ("_RaysAdvancementDuration", Range(0,1)) = 0.7
        _MaxStartingAdvancementDelay ("_MaxStartingAdvancementDelay", Range(0,1)) = 0.2

        [Header(Rays_Angles)]
        _MaxAngleDeviation ("Max Angle Deviation", Range(0, 3.14)) = 1 // in radians

        [Header(Rays_Lengths and Widths)]
        _MaxLength ("Max Length", Float) = 1 // real max lenth being sqrt(2)
        _MinLength ("Min Length", Float) = 0.1
        _MaxWidth ("Max Width", Float) = 0.05
        _MinWidth ("Min Width", Float) = 0.001
        _LengthOverWidthRatio ("Length Over Width Ratio", Float) = 10
        _WidthDeviationMaxPercentage ("Width Deviation Max Percentage", Float) = 0.4

        [Header(Rays_Intensity)]
        _MinIntensity ("Min Intensity", Range(0,1)) = 0.2
        _MaxIntensity ("Max Intensity", Range(0,1)) = 0.8
        _LengthOverWidthIntensityWeight ("Length Over Width Intensity Weight", Float) = .8
        _IntensityMaxVariationThroughTime ("IntensityMaxVariationThroughTime", Float) = .1
        _IntensityVariationSpeedFactor ("IntensityVariationSpeedFactor", int) = 10
        
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

            //Global
            float _ExplosionEpicenterX;        
            float _ExplosionEpicenterY;  
            float _BrigthnessIncreaseSpeed; 
            #define MAX_RAYS 12

            float _RaysSeed;
            int _RaysCount;
            float _MaxAngleDeviation;
            float _MaxLength, _MinLength;
            float _MaxWidth, _MinWidth;
            float _MinIntensity, _MaxIntensity;
            float _LengthOverWidthRatio;
            float _WidthDeviationMaxPercentage;
            float _LengthOverWidthIntensityWeight;
            float _RaysStartingAdvancement;
            float _RaysEndingAdvancement;
            float _IntensityMaxVariationThroughTime;
            int _IntensityVariationSpeedFactor;
            float _MaxStartingAdvancementDelay;
            struct RayData
            {
                float angle;
                float length;
                float width;
                float intensity;
                float delay;
            };
            RayData rays[MAX_RAYS];

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

            float Hash(float x)
            {
                return frac(sin(x) * 43758.5453);
            }

            void ComputeRayAttributes(int i)
            {
                float raySeed = _RaysSeed + i * 12.9898;

                float angleHash     = Hash(raySeed + 17.123);
                float lengthHash    = Hash(raySeed + 43.721);
                float widthHash     = Hash(raySeed + 91.357);
                float intensityHash = Hash(raySeed + 157.891);
                float delayHash = Hash(raySeed + 211.469);

                float angle, length, width, intensity, delay;

                //angle
                float startingAngle = frac(sin(_RaysSeed) * 43758.5453) * 6.2831853;
                float deviation = 2 * (angleHash - 0.5) * _MaxAngleDeviation;
                angle = startingAngle + (6.2831853 * i / _RaysCount) + deviation;

                //length
                length = _MinLength + lengthHash * (_MaxLength - _MinLength);

                //width
                float widthRandomMultiplier = 1 + 2 * (widthHash - 0.5) * _WidthDeviationMaxPercentage;
                width = clamp(widthRandomMultiplier * (length / _LengthOverWidthRatio), _MinWidth, _MaxWidth);

                //intensity
                intensity = _MinIntensity + intensityHash * (_MaxIntensity - _MinIntensity);

                //delay
                delay = delayHash * _MaxStartingAdvancementDelay;

                rays[i].angle = angle;
                rays[i].length = length;
                rays[i].width = width;
                rays[i].intensity = intensity;
                rays[i].delay = delay;
            }

            float4 ComputeLocalRayAttributes(int i, float localAdvancement)
            {
                float4 localRayAttributes;
                float angle = rays[i].angle;
                float length = localAdvancement * rays[i].length;
                float width = localAdvancement * rays[i].width;
                float intensity = localAdvancement * rays[i].intensity + sin(_Time.y * _IntensityVariationSpeedFactor) * _IntensityMaxVariationThroughTime;
                return float4(angle, length, width, intensity);
            }


            float PixelBelongsToRay(float4 RayAttributes, float2 pixel)//RayAttributes = (angle, length, width, intensity) ; 0 edge, 1 at the center
            {
                float tmp1 = cos(RayAttributes.x);
                float tmp2 = sin(RayAttributes.x);
                float2 vectA = float2(tmp1, tmp2);//ray direction
                float2 vectB = float2(-tmp2, tmp1);//second vector making an orthogonal basis of the plane with vectA
                
                float lengthProjection = dot(pixel, vectA);
                float widthAbsProjection = abs(dot(pixel, vectB));

                if (0<=lengthProjection && lengthProjection<=RayAttributes.y && widthAbsProjection<=(RayAttributes.z/2))
                    //return (_LengthOverWidthIntensityWeight * saturate(1 - lengthProjection/RayAttributes.y) + (1 - _LengthOverWidthIntensityWeight) * saturate(1 - widthAbsProjection/(RayAttributes.z/2)));//not bad (with only the first test, sould be added but not quite rays
                    return (_LengthOverWidthIntensityWeight * saturate(1 - lengthProjection/RayAttributes.y) + (1 - _LengthOverWidthIntensityWeight) * saturate(1 - widthAbsProjection/(RayAttributes.z/2)));
                return 0.0;
            }

            float3 IncreaseLuminosity(float3 colorRGB, float amount)
            {
                //compute actual luminosity of my pixel
                float3 hsl = RGBtoHSL(colorRGB);
                float luminosity = hsl.z;

                //deduce the brightness for the actual pixel, capped at 100
                float brightness = saturate(luminosity + amount);
                
                //final colour
                return HSLtoRGB(float3(hsl.x, hsl.y, brightness));
            }

            float3 Blast(float3 col)
            {
            }
            
            float3 Rays(float3 col)
            {
            }

            float3 LargeRays(float3 col)
            {
            }


            half4 Frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

                // --- General Explosion ---
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
                float localAdvancement;
                if (localRange < explosionRange)
                    localAdvancement = (explosionRange - localRange) / propagationSpeed;
                else
                    localAdvancement = 0;
                //final color
                //col = IncreaseLuminosity(col, localAdvancement * _BrigthnessIncreaseSpeed);
                

                // --- Rays ---
                if (_RaysStartingAdvancement<=_ExplosionGlobalAdvancement && _ExplosionGlobalAdvancement<=_RaysEndingAdvancement)
                {                    
                    for (int i = 0; i < _RaysCount; i++)
                    {                        
                        ComputeRayAttributes(i);
                        if (_RaysStartingAdvancement + rays[i].delay <= _ExplosionGlobalAdvancement)
                        {
                            float localStart = _RaysStartingAdvancement + rays[i].delay;
                            float localAdvancement = (_ExplosionGlobalAdvancement - localStart)/(_RaysEndingAdvancement - localStart);
                            float4 localRayAttributes;
                            localRayAttributes = ComputeLocalRayAttributes(i, localAdvancement);

                            float brightenPixel = PixelBelongsToRay(localRayAttributes, (uv-float2(_ExplosionEpicenterX, _ExplosionEpicenterY)));
                            if (brightenPixel > 0)
                                col = IncreaseLuminosity(col, brightenPixel * localRayAttributes.w);
                        }
                    }
                }

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}

