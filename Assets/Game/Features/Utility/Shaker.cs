using System.Collections;
using UnityEngine;

namespace Assets.Game.View
{
    public static class Shaker
    {
        /// <summary>
        /// OBJECT MUST REMAIN FIXED THROUGHOUT THE SHAKE
        /// </summary>
        public static IEnumerator Shake(Transform entityView, float time, float intensity)
        {
            Vector3 initPos = entityView.localPosition;

            Vector2 localShakePos = Vector2.zero;

            float t = time;
            while (t >= 0)
            {
                Debug.Log(localShakePos);
                Vector2 random = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * intensity;

                float maxBound = intensity * t / time;

                random = new Vector2(Mathf.Clamp(localShakePos.x + random.x, -maxBound, +maxBound), Mathf.Clamp(localShakePos.y + random.y, -maxBound, +maxBound)) - localShakePos;

                entityView.localPosition += new Vector3(random.x, random.y, 0);

                localShakePos += random;

                t -= Time.deltaTime;
                yield return 0;
            }

            entityView.localPosition -= new Vector3(localShakePos.x, localShakePos.y);
            Debug.Log(entityView.localPosition);
        }
    }
}
