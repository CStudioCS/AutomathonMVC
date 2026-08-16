Shader "Automathon/NeonZapper"
{
    // Jetpack-Joyride style zapper: two orb electrodes at the ends of the box's LONG axis,
    // joined by a thin laser beam. Everything is a signed-distance field evaluated in the
    // quad's local space, so a single sprite quad + MaterialPropertyBlock is enough.
    //
    // The quad is _Pad world units larger than the wall on every side (so the glow has room
    // to bleed outside the collider); _Size stays the TRUE wall size and drives the SDF.
    Properties
    {
        [HDR] _BeamColor    ("Beam Color", Color) = (0.10, 0.80, 1.00, 1)
        [HDR] _CoreColor    ("Core Color", Color) = (1, 1, 1, 1)

        _Size               ("Size (w,h) world units", Vector) = (1, 1, 0, 0)
        _Pad                ("Glow padding (world units)", Float) = 0.5

        _BeamThickness      ("Beam radius (x half-thickness)", Range(0.05, 1)) = 0.30
        _OrbRadius          ("Orb radius (x half-thickness)", Range(0.2, 1.5)) = 1.00
        _CoreSharp          ("Beam core sharpness", Range(0.05, 1)) = 0.30
        _OrbCoreSharp       ("Orb core sharpness", Range(0.05, 1)) = 0.18
        _Falloff            ("Halo falloff (world units)", Float) = 0.15

        _BodyIntensity      ("Body intensity", Float) = 1.15
        _GlowIntensity      ("Halo intensity", Float) = 1.00
        _CoreIntensity      ("Core intensity", Float) = 1.15

        _ArcAmp             ("Arc amplitude (0 = pure laser)", Range(0, 0.8)) = 0.12
        _ArcFreq            ("Arc frequency", Float) = 6.0
        _ArcSpeed           ("Arc speed", Float) = 4.0
        _Pulse              ("Pulse amount", Range(0, 0.6)) = 0.12
        _PulseSpeed         ("Pulse speed", Float) = 8.0
        _Phase              ("Per-instance phase", Float) = 0

        [Header(Impact)]
        _ImpactSpeed        ("Wave speed (world units/s)", Float) = 12.0
        _ImpactWidth        ("Front width (world units)", Float) = 0.25
        _ImpactSpread       ("Front dispersion", Float) = 1.5
        _ImpactDecay        ("Decay time constant (s)", Float) = 0.45
        _ImpactSwell        ("Beam swell", Range(0, 3)) = 1.20
        _ImpactBright       ("Brightness boost", Range(0, 6)) = 2.50
        _ImpactArc          ("Arc boost", Range(0, 6)) = 2.00
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend One One       // additive
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BeamColor;
                float4 _CoreColor;
                float4 _Size;
                float  _Pad;
                float  _BeamThickness;
                float  _OrbRadius;
                float  _CoreSharp;
                float  _OrbCoreSharp;
                float  _Falloff;
                float  _BodyIntensity;
                float  _GlowIntensity;
                float  _CoreIntensity;
                float  _ArcAmp;
                float  _ArcFreq;
                float  _ArcSpeed;
                float  _Pulse;
                float  _PulseSpeed;
                float  _Phase;
                float  _ImpactSpeed;
                float  _ImpactWidth;
                float  _ImpactSpread;
                float  _ImpactDecay;
                float  _ImpactSwell;
                float  _ImpactBright;
                float  _ImpactArc;
            CBUFFER_END

            // Distance to an axis-aligned box; with one half-extent = 0 this is the
            // distance to a segment, which is exactly what the beam centreline needs.
            float sdBox(float2 p, float2 b)
            {
                float2 q = abs(p) - b;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
            }

            // Impact ring buffer, written from C# via MaterialPropertyBlock.SetVectorArray.
            // x = position along the long axis (world units), y = _Time.y at impact, z = strength.
            // Kept outside UnityPerMaterial: arrays are not SRP-batcher compatible.
            #define IMPACT_SLOTS 4
            float4 _Impacts[IMPACT_SLOTS];

            // R -> [-E, E] triangle fold. Reflecting the FRONT POSITION instead of adding one
            // gaussian per bounce gives infinite reflections off both orbs for ~4 ALU.
            float fold(float x, float E)
            {
                float t = frac(x / (4.0 * E) + 0.25);
                return E * (1.0 - 4.0 * abs(t - 0.5));
            }

            // Dispersive bidirectional wave: tight and bright at t=0 (both fronts coincide on the
            // impact point -> double amplitude punch), widening and fading as it travels.
            float impactWave(float along, float E)
            {
                float acc = 0.0;
                [unroll]
                for (int k = 0; k < IMPACT_SLOTS; k++)
                {
                    float age = _Time.y - _Impacts[k].y;
                    float amp = _Impacts[k].z * exp(-max(age, 0.0) / _ImpactDecay);
                    float w   = _ImpactWidth * (1.0 + age * _ImpactSpread);
                    float dR  = along - fold(_Impacts[k].x + _ImpactSpeed * age, E);
                    float dL  = along - fold(_Impacts[k].x - _ImpactSpeed * age, E);
                    float inv = 1.0 / (2.0 * w * w);
                    acc += amp * (exp(-dR * dR * inv) + exp(-dL * dL * inv));
                }
                return acc;
            }

            // Exponential halo renormalised so it hits exactly 0 at the quad border.
            // Raw exp(-x/f) still carries exp(-_Pad/_Falloff) there (3.6% at the default
            // params); under additive blending that residual stops dead at the mesh edge
            // and the padded quad shows up as a bright rectangle around every wall.
            // `margin` = shortest distance from the lit shape to the quad edge.
            float haloFalloff(float x, float margin)
            {
                float g0 = exp(-margin / _Falloff);
                return saturate((exp(-x / _Falloff) - g0) / (1.0 - g0));
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv    = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Quad-local position in world units, centred on the wall.
                float2 quad = _Size.xy + 2.0 * _Pad;
                float2 p    = (IN.uv - 0.5) * quad;

                float2 hs = _Size.xy * 0.5;
                float  R  = min(hs.x, hs.y);          // half thickness -> orb radius unit
                float  E  = max(hs.x, hs.y);          // half length of the box (fold boundary)
                float  L  = E - R;                    // half distance between orb centres
                bool   horiz = _Size.x >= _Size.y;

                float along = horiz ? p.x : p.y;
                float perp  = horiz ? p.y : p.x;

                float wave = impactWave(along, E);

                // Electric wobble on the beam, damped to zero at the orbs so it stays attached.
                float damp = saturate(1.0 - (along * along) / max(L * L, 1e-6));
                float arc  = _ArcAmp * (1.0 + wave * _ImpactArc);
                float wob  = sin(along * _ArcFreq + _Time.y * _ArcSpeed + _Phase) * arc * R * damp;
                perp -= wob;

                float2 pw = horiz ? float2(along, perp) : float2(perp, along);
                float2 a  = horiz ? float2(L, 0.0)      : float2(0.0, L);

                float dSeg = sdBox(pw, a);                  // -> beam centreline
                float dOrb = length(abs(p) - a);            // -> nearest orb centre

                // The wave swells the beam. Capped so the lit surface never leaves the collider
                // box, which keeps `margin` below a valid conservative bound.
                float swell = 1.0 + wave * _ImpactSwell;
                float rb = min(R * _BeamThickness * swell, R);
                float ro = min(R * _OrbRadius * swell, R * max(_OrbRadius, 1.0));
                float aa = fwidth(dSeg) + 1e-5;

                float body = max(1.0 - smoothstep(rb - aa, rb + aa, dSeg),
                                 1.0 - smoothstep(ro - aa, ro + aa, dOrb));

                float core = exp(-dSeg / max(rb * _CoreSharp,    1e-4))
                           + exp(-dOrb / max(ro * _OrbCoreSharp, 1e-4)) * 0.85;

                // Orbs bulge outside the wall box when _OrbRadius > 1, which eats into the pad.
                float margin = _Pad - R * max(_OrbRadius - 1.0, 0.0);
                float halo = haloFalloff(max(dSeg - rb, 0.0), margin)
                           + haloFalloff(max(dOrb - ro, 0.0), margin) * 1.3;

                float pulse = 1.0 + _Pulse * sin(_Time.y * _PulseSpeed + _Phase * 3.7);

                half3 col = _BeamColor.rgb * (body * _BodyIntensity + halo * _GlowIntensity)
                          + _CoreColor.rgb * core * _CoreIntensity;

                col *= pulse * (1.0 + wave * _ImpactBright) * IN.color.rgb * IN.color.a;   // SpriteRenderer.color = tint / fade
                return half4(col, 1.0);                     // additive: alpha unused
            }
            ENDHLSL
        }
    }
    Fallback Off
}
