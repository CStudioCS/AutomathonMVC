using Automathon.Game.View;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomPulse : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private float pulsePeriod = 10f;
    [SerializeField] private float minIntensity = 1f;
    [SerializeField] private float maxIntensity = 10f;

    private Bloom bloom;

    void Start()
    {
        if (!volume.profile.TryGet(out bloom))
        {
            enabled = false;
            return;
        }
        bloom.intensity.overrideState = true;
    }
    void Update()
    {
        float ratio = CalculateRatio();
        bloom.intensity.value = Mathf.Lerp(minIntensity, maxIntensity, ratio);
    }

    private float CalculateRatio()
    {
        return ViewMath.TriangleFunction(pulsePeriod, 1f);
    }
}
