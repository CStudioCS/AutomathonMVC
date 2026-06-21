using Assets.Game.View;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    public void CameraShake(float shakeTime, float shakeIntensity)
    {
        StartCoroutine(Shaker.Shake(transform, shakeTime, shakeIntensity));
    }
    public void CameraTranslate(Vector2 dir, int miliTime, float distance)
    {
        StartCoroutine(Shaker.Translate(transform, dir, miliTime, distance));
    }
}
