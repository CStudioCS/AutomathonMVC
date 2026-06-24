Shader "Automathon/HealthBar"
{
    Properties
    {
        _Fill        ("Fill", Range(0,1)) = 1
        _FillChip    ("Chip Fill", Range(0,1)) = 1
        _Aspect      ("Aspect (w/h)", Float) = 5.7
        _AlphaMul    ("Alpha Mul", Range(0,1)) = 1

        _ColorFull   ("Color Full", Color) = (0.30,0.85,0.40,1)
        _ColorMid    ("Color Mid",  Color) = (0.95,0.80,0.25,1)
        _ColorLow    ("Color Low",  Color) = (0.90,0.25,0.25,1)
        _ChipColor   ("Chip Color", Color) = (1,1,1,0.9)
        _BackColor   ("Background", Color) = (0.08,0.08,0.10,0.85)
        _BorderColor ("Border",     Color) = (0.02,0.02,0.03,1)

        _BorderThickness ("Border Thickness", Range(0,0.5)) = 0.06
        _Radius          ("Corner Radius",    Range(0,0.5)) = 0.25
        _MidPoint        ("Ramp Midpoint",    Range(0,1))   = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                float _Fill, _FillChip, _Aspect, _AlphaMul;
                float _BorderThickness, _Radius, _MidPoint;
                float4 _ColorFull, _ColorMid, _ColorLow, _ChipColor, _BackColor, _BorderColor;
            CBUFFER_END

            // SDF rounded box, négatif à l'intérieur. b = demi-taille, r = rayon de coin.
            float sdRoundBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - (b - r);
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Forme : espace centré, corrigé par l'aspect -> coins circulaires en monde
                float2 p        = (uv - 0.5) * float2(_Aspect, 1.0);
                float2 halfSize = float2(_Aspect, 1.0) * 0.5;
                float  r        = min(_Radius, 0.5);

                float dOuter    = sdRoundBox(p, halfSize, r);
                float shapeMask = saturate(0.5 - dOuter / (fwidth(dOuter) + 1e-5));
                if (shapeMask <= 0.0) discard;

                // Bordure : box interne inset
                float bt        = _BorderThickness;
                float dInner    = sdRoundBox(p, halfSize - bt, max(r - bt, 0.0));
                float innerMask = saturate(0.5 - dInner / (fwidth(dInner) + 1e-5));

                // Régions de remplissage le long de u (AA sur le bord)
                float ufw    = fwidth(uv.x) + 1e-5;
                float inFill = smoothstep(_Fill     + ufw, _Fill     - ufw, uv.x);
                float inChip = smoothstep(_FillChip + ufw, _FillChip - ufw, uv.x) * (1.0 - inFill);

                // Rampe de couleur selon la vie courante (_Fill)
                float h = _Fill;
                half3 ramp = h < _MidPoint
                    ? lerp(_ColorLow.rgb, _ColorMid.rgb,  saturate(h / max(_MidPoint, 1e-4)))
                    : lerp(_ColorMid.rgb, _ColorFull.rgb, saturate((h - _MidPoint) / max(1.0 - _MidPoint, 1e-4)));
                ramp *= lerp(0.88, 1.12, uv.y); // léger gloss vertical

                // Composition intérieure
                half3 interior = _BackColor.rgb;
                interior = lerp(interior, _ChipColor.rgb, inChip);
                interior = lerp(interior, ramp,           inFill);

                half3 col = lerp(_BorderColor.rgb, interior, innerMask);

                float regionA = _BackColor.a;
                regionA = lerp(regionA, _ChipColor.a, inChip);
                regionA = lerp(regionA, 1.0,          inFill);
                float alpha = lerp(_BorderColor.a, regionA, innerMask) * shapeMask * _AlphaMul;

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}