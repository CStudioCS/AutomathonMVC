using UnityEngine;
using UnityEngine.VFX;

public class VFXDestroyOnDone : MonoBehaviour
{
    private VisualEffect visualEffect;
    private void Awake()
    {
        visualEffect = GetComponent<VisualEffect>();
    }

    private void Update()
    {
        if (visualEffect.aliveParticleCount == 0)
            Destroy(gameObject);
    }
}
