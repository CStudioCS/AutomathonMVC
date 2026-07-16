using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Automathon.Game
{
    // Screen-wide glitch pulses fired by small game actions. Each pulse runs a short bell-curve
    // envelope (0 -> peak -> 0 over its profile's duration) reaching, at the peak, the profile's
    // values across three channels: the VHS shader (line-glitch intensity + horizontal line
    // offset), URP chromatic aberration, and URP lens distortion. Two profiles: Light (most
    // actions) and Heavy (explosions). Views call Trigger() / TriggerHeavy(); no instance = no-op.
    public class ScreenGlitch : MonoBehaviour
    {
        [System.Serializable]
        public struct GlitchProfile
        {
            public float duration;            // bell length (s)
            public float vhsGlitchIntensity;  // -> VHS _GlitchIntensity (how many scanlines jump)
            public float maxLineOffset;       // -> VHS _GlitchLineOffset (max horizontal shift, uv)
            public float chromaticAberration; // added on top of the base CA intensity
            public float lensDistortion;      // added on top of the base lens distortion intensity
        }

        [Header("Targets")]
        [SerializeField] private Material vhsMaterial;
        [SerializeField] private Volume postVolume;

        [Header("Profiles")]
        [SerializeField] private GlitchProfile light = new GlitchProfile
        { duration = 0.1f, vhsGlitchIntensity = 0.25f, maxLineOffset = 0.0025f, chromaticAberration = 0.5f, lensDistortion = -0.25f };
        [SerializeField] private GlitchProfile heavy = new GlitchProfile
        { duration = 0.3f, vhsGlitchIntensity = 1.5f, maxLineOffset = 0.03f, chromaticAberration = 1f, lensDistortion = -0.5f };

        private static ScreenGlitch instance;
        private static readonly int GlitchIntensityId = Shader.PropertyToID("_GlitchIntensity");
        private static readonly int GlitchLineOffsetId = Shader.PropertyToID("_GlitchLineOffset");

        private ChromaticAberration ca;
        private LensDistortion ld;
        private float baseCa, baseLd;

        private struct Pulse { public float Start; public GlitchProfile P; }
        private readonly List<Pulse> pulses = new();

        /// <summary>Light glitch (most actions). No-op if no ScreenGlitch exists.</summary>
        public static void Trigger() => instance?.AddPulse(false);

        /// <summary>Heavy glitch (explosions). No-op if no ScreenGlitch exists.</summary>
        public static void TriggerHeavy() => instance?.AddPulse(true);

        private void AddPulse(bool heavyTier)
            => pulses.Add(new Pulse { Start = Time.time, P = heavyTier ? heavy : light });

        private void Awake() => instance = this;

        private void OnEnable()
        {
            // Volume.profile is a per-volume instance, so animating it never dirties the shared asset.
            if (postVolume != null && postVolume.profile != null)
            {
                postVolume.profile.TryGet(out ca);
                postVolume.profile.TryGet(out ld);
            }
            if (ca != null) baseCa = ca.intensity.value;
            if (ld != null) baseLd = ld.intensity.value;
        }

        private void OnDisable()
        {
            // Leave nothing glitched behind when play stops or the object is disabled.
            if (ca != null) ca.intensity.value = baseCa;
            if (ld != null) ld.intensity.value = baseLd;
            if (vhsMaterial != null)
            {
                vhsMaterial.SetFloat(GlitchIntensityId, 0f);
                vhsMaterial.SetFloat(GlitchLineOffsetId, 0f);
            }
            pulses.Clear();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Update()
        {
            // Per-channel peak across all active pulses (overlapping pulses combine by max, so the
            // result never exceeds a single pulse's peak — a light burst can't dampen a heavy one).
            float vhsI = 0f, lineOff = 0f, caBoost = 0f, lensBoost = 0f;
            for (int i = pulses.Count - 1; i >= 0; i--)
            {
                GlitchProfile p = pulses[i].P;
                float u = (Time.time - pulses[i].Start) / Mathf.Max(p.duration, 1e-4f);
                if (u >= 1f) { pulses.RemoveAt(i); continue; }
                float b = Mathf.Sin(u * Mathf.PI); // 0 -> 1 -> 0
                vhsI = Mathf.Max(vhsI, b * p.vhsGlitchIntensity);
                lineOff = Mathf.Max(lineOff, b * p.maxLineOffset);
                caBoost = Mathf.Max(caBoost, b * p.chromaticAberration);
                float lb = b * p.lensDistortion;
                if (Mathf.Abs(lb) > Mathf.Abs(lensBoost)) lensBoost = lb; // strongest signed value wins
            }

            if (vhsMaterial != null)
            {
                vhsMaterial.SetFloat(GlitchIntensityId, vhsI);
                vhsMaterial.SetFloat(GlitchLineOffsetId, lineOff);
            }
            if (ca != null) ca.intensity.value = Mathf.Clamp01(baseCa + caBoost);
            if (ld != null) ld.intensity.value = Mathf.Clamp(baseLd + lensBoost, -1f, 1f);
        }
    }
}
