using System.Collections.Generic;
using UnityEngine;

public class LightExplosion : MonoBehaviour
{
    private float explosionStartTime;
    private float explosionEndTime;
    private bool isLightExplosionActive = false;

    [SerializeField] private Material lightExplosionMaterial;

    public static LightExplosion instance;


    //General
    public static readonly int _epicenterXId = Shader.PropertyToID("_ExplosionEpicenterX");
    public static readonly int _epicenterYId = Shader.PropertyToID("_ExplosionEpicenterY");

    public static readonly int _ActivateBlastID = Shader.PropertyToID("_ActivateBlast");
    public static readonly int _ActivateRaysID = Shader.PropertyToID("_ActivateRays");
    public static readonly int _ActivateLargeRaysID = Shader.PropertyToID("_ActivateLargeRays");

    public static readonly int _BlastAdvancementID = Shader.PropertyToID("_BlastAdvancement");
    public static readonly int _RaysAdvancementID = Shader.PropertyToID("_RaysAdvancement");
    public static readonly int _LargeRaysAdvancementID = Shader.PropertyToID("_LargeRaysAdvancement");

    //Blast

    //Rays

    //LargeRays


    [SerializeField] private List<LightExplosionEffectData> activeLightExplosionEffects;

    public enum EffectType { Blast, Rays, LargeRays }

    public struct LightExplosionEffectData
    {
        public EffectType effect;
        public float start;//global light explosion duration percentage
        public float end;
        public AnimationCurve effectProgressToLocalAdvancement;
    };

    private void Awake() => instance = this;

    private void OnEnable()
    {
        StopAllEffects();
    }

    private void OnDisable()
    {
        lightExplosionMaterial.SetFloat(_epicenterXId, 0f);
        lightExplosionMaterial.SetFloat(_epicenterYId, 0f);
        StopAllEffects();
    }

    private void StopAllEffects()
    {
        isLightExplosionActive = false;
        lightExplosionMaterial.SetInt(_ActivateBlastID, 0);
        lightExplosionMaterial.SetInt(_ActivateRaysID, 0);
        lightExplosionMaterial.SetInt(_ActivateLargeRaysID, 0);
        lightExplosionMaterial.SetFloat(_BlastAdvancementID, 0f);
        lightExplosionMaterial.SetFloat(_RaysAdvancementID, 0f);
        lightExplosionMaterial.SetFloat(_LargeRaysAdvancementID, 0f);
    }

    public void LaunchLightExplosion(float epicenterX, float epicenterY, float explosionDuration)
    {
        lightExplosionMaterial.SetFloat("_RaysSeed", Time.time);
        lightExplosionMaterial.SetFloat(_epicenterXId, epicenterX);
        lightExplosionMaterial.SetFloat(_epicenterYId, epicenterY);
        explosionStartTime = Time.time;
        explosionEndTime = explosionStartTime + explosionDuration;
        isLightExplosionActive = true;
    }

    private void StopEffect(EffectType effectType)
    {
        switch (effectType)
        {
            case (EffectType.Blast):
                lightExplosionMaterial.SetInt(_ActivateBlastID, 0);
                break;
            case (EffectType.Rays):
                lightExplosionMaterial.SetInt(_ActivateRaysID, 0);
                break;
            case (EffectType.LargeRays):
                lightExplosionMaterial.SetInt(_ActivateLargeRaysID, 0);
                break;
        }
    }

    private void StartEffect(EffectType effectType)
    {
        switch (effectType)
        {
            case (EffectType.Blast):
                lightExplosionMaterial.SetInt(_ActivateBlastID, 1);
                break;
            case (EffectType.Rays):
                lightExplosionMaterial.SetInt(_ActivateRaysID, 1);
                break;
            case (EffectType.LargeRays):
                lightExplosionMaterial.SetInt(_ActivateLargeRaysID, 1);
                break;
        }
    }

    private void SetBlastParams()
    {

    }

    private void SetRaysParams()
    {

    }

    private void SetLargeRaysParams()
    {

    }

    private void SetParams(EffectType effectType)
    {
        switch (effectType)
        {
            case (EffectType.Blast):
                SetBlastParams();
                break;
            case (EffectType.Rays):
                SetRaysParams();
                break;
            case (EffectType.LargeRays):
                SetLargeRaysParams();
                break;
        }
    }
    private void SetLocalAdvancement(EffectType effectType, float localAdvancement)
    {
        switch (effectType)
        {
            case (EffectType.Blast):
                lightExplosionMaterial.SetFloat(_BlastAdvancementID, localAdvancement);
                break;
            case (EffectType.Rays):
                lightExplosionMaterial.SetFloat(_RaysAdvancementID, localAdvancement);
                break;
            case (EffectType.LargeRays):
                lightExplosionMaterial.SetFloat(_LargeRaysAdvancementID, localAdvancement);
                break;
        }
    }

    private void Update()
    {
        if (isLightExplosionActive)
        {
            if (Time.time > explosionEndTime)
            {
                isLightExplosionActive = false;
            }
            else
            {
                float t = (Time.time - explosionStartTime) / (explosionEndTime - explosionStartTime);
                int nbEff = activeLightExplosionEffects.Count;
                for (int i = nbEff - 1; i >= 0; i--)
                {
                    LightExplosionEffectData effectData = activeLightExplosionEffects[i];
                    float progress = (t - effectData.start) / (effectData.end - effectData.start);
                    if (progress > 1f || progress < 0f)
                    {
                        StopEffect(effectData.effect);
                        activeLightExplosionEffects.RemoveAt(i);
                    }
                    else
                    {
                        float localAdvancement = effectData.effectProgressToLocalAdvancement.Evaluate(progress);
                        SetParams(effectData.effect);
                        SetLocalAdvancement(effectData.effect, localAdvancement);
                        StartEffect(effectData.effect);
                    }
                }
            }
        }
    }
}