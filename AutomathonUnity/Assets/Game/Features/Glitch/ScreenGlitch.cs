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

            public float vhsGlitchDensity;  // -> VHS _GlitchIntensity (how many scanlines jump)
            public float maxLineOffset;       // -> VHS _GlitchLineOffset (max horizontal shift, uv)
            public float vhsGlitchSpeed;        // -> VHS _GlitchSpeed (how fast the scanlines jump)

            public float chromaticAberrationBoost; // added on top of the base CA intensity
            public float lensDistortionBoost;      // added on top of the base lens distortion intensity

            public float maxStaticNoiseProbability; // VHS _StaticNoiseProbability (how often static noise appears when at the peak of the glitch) (0-1)
            public float maxGlitchDesaturation; // VHS _GlitchDesaturation (how desaturated the glitch is at the peak) (0-1)
            public float onlyStaticNoise; // VHS _OnlyStaticNoise (if true, only static noise is shown, no scanline glitches)
        }

        [System.Serializable]
        public struct SoundProfile
        {
            public string soundName;
            public float minVolume;
            public float maxVolume;
        }

        [Header("Targets")]
        [SerializeField] private Material vhsMaterial;
        [SerializeField] private Volume postVolume;

        [Header("Glitch Profiles")]
        [SerializeField]
        public GlitchProfile light = new GlitchProfile
        { duration = 0.1f, vhsGlitchDensity = 0.25f, maxLineOffset = 0.0025f, vhsGlitchSpeed = 60f, chromaticAberrationBoost = 0.5f, lensDistortionBoost = -0.25f, maxStaticNoiseProbability = 0.02f, maxGlitchDesaturation = 0.4f, onlyStaticNoise = 1f };
        [SerializeField]
        public GlitchProfile heavy = new GlitchProfile
        { duration = 0.3f, vhsGlitchDensity = 1.5f, maxLineOffset = 0.03f, vhsGlitchSpeed = 60f, chromaticAberrationBoost = 1f, lensDistortionBoost = -0.5f, maxStaticNoiseProbability = 0.1f, maxGlitchDesaturation = 0.8f, onlyStaticNoise = 1f };
        [SerializeField]
        public GlitchProfile menuTransition = new GlitchProfile
        { duration = 2f, vhsGlitchDensity = 10f, maxLineOffset = 0.03f, vhsGlitchSpeed = 60f, chromaticAberrationBoost = 1f, lensDistortionBoost = -0.5f, maxStaticNoiseProbability = 0.4f, maxGlitchDesaturation = 1.0f, onlyStaticNoise = 0f };

        [Header("Sound Profiles")]
        [SerializeField]
        private SoundProfile lightSound = new SoundProfile { soundName = null, minVolume = 0f, maxVolume = 0f };
        [SerializeField]
        private SoundProfile heavySound = new SoundProfile { soundName = null, minVolume = 0f, maxVolume = 0f };
        [SerializeField]
        private SoundProfile menuTransitionSound = new SoundProfile { soundName = "MenuTransition_Static", minVolume = 0.2f, maxVolume = 0.6f };

        private enum ProfileType { Light, Heavy, MenuTransition }


        public static ScreenGlitch instance;
        private static readonly int GlitchDensityId = Shader.PropertyToID("_GlitchLineDensity");
        private static readonly int GlitchLineOffsetId = Shader.PropertyToID("_GlitchLineOffset");
        private static readonly int GlitchSpeedId = Shader.PropertyToID("_GlitchSpeed");
        private static readonly int StaticNoiseProbabilityId = Shader.PropertyToID("_StaticNoiseProbability");
        private static readonly int GlitchDesaturationId = Shader.PropertyToID("_GlitchDesaturation");
        private static readonly int OnlyStaticNoiseId = Shader.PropertyToID("_OnlyStaticNoise");

        private ChromaticAberration ca;
        private LensDistortion ld;
        private float baseCa, baseLd;

        private struct Pulse { public float Start; public GlitchProfile P; public SoundProfile S; public int soundId; }
        private readonly List<Pulse> pulses = new();

        /// <summary>Light glitch (most actions). No-op if no ScreenGlitch exists.</summary>
        public static void Trigger()
        {
            instance?.AddPulse(ProfileType.Light);
        }

        /// <summary>Heavy glitch (explosions). No-op if no ScreenGlitch exists.</summary>
        public static void TriggerHeavy()
        {
            instance?.AddPulse(ProfileType.Heavy);
        }

        public static void TriggerMenuTransition()
        {
            instance?.AddPulse(ProfileType.MenuTransition);
        }

        private void AddPulse(ProfileType profileType)
        {
            SoundProfile S = GetSoundProfile(profileType);
            if (string.IsNullOrEmpty(S.soundName))
            {
                pulses.Add(new Pulse { Start = Time.time, P = GetGlitchProfile(profileType), S = S, soundId = -1 });
            }
            else
            {
                int soundId = SoundManager.instance.PlaySound(S.soundName, playOneShot: false, startingVolume: S.minVolume);
                pulses.Add(new Pulse { Start = Time.time, P = GetGlitchProfile(profileType), S = S, soundId = soundId });
            }
        }

        private GlitchProfile GetGlitchProfile(ProfileType profileType)
        {
            return profileType switch
            {
                ProfileType.Light => light,
                ProfileType.Heavy => heavy,
                ProfileType.MenuTransition => menuTransition,
            };
        }

        private SoundProfile GetSoundProfile(ProfileType profileType)
        {
            return profileType switch
            {
                ProfileType.Light => lightSound,
                ProfileType.Heavy => heavySound,
                ProfileType.MenuTransition => menuTransitionSound,
            };
        }


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
                vhsMaterial.SetFloat(GlitchDensityId, 0f);
                vhsMaterial.SetFloat(GlitchLineOffsetId, 0f);
                vhsMaterial.SetFloat(GlitchSpeedId, 0f);
                vhsMaterial.SetFloat(GlitchDesaturationId, 0f);
                vhsMaterial.SetFloat(GlitchDesaturationId, 0f);
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
            float vhsD = 0f, lineOff = 0f, caBoost = 0f, lensBoost = 0f, vhsSpeed = 60f, snp = 0f, desat = 0f, onlyStatic = 1f;
            for (int i = pulses.Count - 1; i >= 0; i--)
            {
                GlitchProfile p = pulses[i].P;
                float u = (Time.time - pulses[i].Start) / Mathf.Max(p.duration, 1e-4f);
                if (u >= 1f)
                {
                    if (pulses[i].soundId >= 0)
                        SoundManager.instance.StopSound(pulses[i].soundId);
                    pulses.RemoveAt(i);
                    continue;
                }
                float b = Mathf.Sin(u * Mathf.PI); // 0 -> 1 -> 0
                vhsD = Mathf.Max(vhsD, b * p.vhsGlitchDensity);
                lineOff = Mathf.Max(lineOff, b * p.maxLineOffset);
                caBoost = Mathf.Max(caBoost, b * p.chromaticAberrationBoost);
                vhsSpeed = Mathf.Max(vhsSpeed, b * p.vhsGlitchSpeed);

                float lb = b * p.lensDistortionBoost;
                if (Mathf.Abs(lb) > Mathf.Abs(lensBoost)) lensBoost = lb; // strongest signed value wins

                snp = Mathf.Max(snp, b * p.maxStaticNoiseProbability);
                desat = Mathf.Max(desat, b * p.maxGlitchDesaturation);

                onlyStatic = Mathf.Min(onlyStatic, p.onlyStaticNoise);

                if (pulses[i].soundId >= 0)
                    SoundManager.instance.SetVolume(pulses[i].soundId, Mathf.Lerp(pulses[i].S.minVolume, pulses[i].S.maxVolume, b));
            }

            if (vhsMaterial != null)
            {
                vhsMaterial.SetFloat(GlitchDensityId, vhsD);
                vhsMaterial.SetFloat(GlitchLineOffsetId, lineOff);
                vhsMaterial.SetFloat(GlitchSpeedId, vhsSpeed); // Set a default speed
                vhsMaterial.SetFloat(StaticNoiseProbabilityId, snp);
                vhsMaterial.SetFloat(GlitchDesaturationId, desat);
                vhsMaterial.SetFloat(OnlyStaticNoiseId, onlyStatic);

            }
            if (ca != null) ca.intensity.value = Mathf.Clamp01(baseCa + caBoost);
            if (ld != null) ld.intensity.value = Mathf.Clamp(baseLd + lensBoost, -1f, 1f);
        }
    }
}
