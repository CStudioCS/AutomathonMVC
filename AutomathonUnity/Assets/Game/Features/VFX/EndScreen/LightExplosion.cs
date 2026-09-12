using System.Collections.Generic;
using UnityEngine;

public class LightExplosion : MonoBehaviour
{
    [SerializeField]
    private float _explosionGlobalAdvancement = 0f;
    [SerializeField]
    private float _advancementForFullScreen = 0.8f;
    [SerializeField]
    private float _brigthnessIncreaseSpeed = 5f;


    [SerializeField] private Material lightExplosionMaterial;

    public static LightExplosion instance;
    private static readonly int _globalAdvancementID = Shader.PropertyToID("_ExplosionGlobalAdvancement");

    public static readonly int _brigthnessIncreaseSpeedID = Shader.PropertyToID("_BrigthnessIncreaseSpeed");
    public static readonly int _advancementForFullScreenID = Shader.PropertyToID("_AdvancementForFullScreen");
    public static readonly int _epicenterXId = Shader.PropertyToID("_ExplosionEpicenterX");
    public static readonly int _epicenterYId = Shader.PropertyToID("_ExplosionEpicenterY");
    private void Awake() => instance = this;

    private List<LightExplosionData> activeLightExplosions = new();

    public struct LightExplosionData
    {
        public float advancementForFullScreen;
        public float brigthnessIncreaseSpeed;
        public float epicenterX;
        public float epicenterY;
        public float explosionDuration;
        public float startTime;
    }

    private void OnEnable()
    {
        lightExplosionMaterial.SetFloat(_globalAdvancementID, 0f);
        lightExplosionMaterial.SetFloat(_brigthnessIncreaseSpeedID, _brigthnessIncreaseSpeed);
        lightExplosionMaterial.SetFloat(_advancementForFullScreenID, _advancementForFullScreen);
    }

    private void OnDisable()
    {
        lightExplosionMaterial.SetFloat(_globalAdvancementID, 0f);
        lightExplosionMaterial.SetFloat(_epicenterXId, 0f);
        lightExplosionMaterial.SetFloat(_epicenterYId, 0f);
    }

    public void LaunchLightExplosion(float epicenterX, float epicenterY, float duration)
    {
        activeLightExplosions.Add(new LightExplosionData
        {
            advancementForFullScreen = _advancementForFullScreen,
            brigthnessIncreaseSpeed = _brigthnessIncreaseSpeed,
            epicenterX = epicenterX,
            epicenterY = epicenterY,
            explosionDuration = duration,
            startTime = Time.time
        });
    }

    private void Update()//inutile, il n'y en aura qu'une et là ça ne marche pas trop avec plusieurs puisqu'on ne garde qu'un épicentre
    {
        int nbExpl = activeLightExplosions.Count;
        for (int i = nbExpl - 1; i >= 0; i--)
        {
            LightExplosionData exp = activeLightExplosions[i];
            float temp = (Time.time - exp.startTime) / exp.explosionDuration;
            if (temp > 1f)
            {
                activeLightExplosions.RemoveAt(i);
            }
            float t = Mathf.Sin(Mathf.PI * temp);
            lightExplosionMaterial.SetFloat(_globalAdvancementID, t);
            lightExplosionMaterial.SetFloat(_epicenterXId, exp.epicenterX);
            lightExplosionMaterial.SetFloat(_epicenterYId, exp.epicenterY);
        }
    }
}
