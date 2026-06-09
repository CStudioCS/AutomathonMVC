using System.Collections;
using UnityEngine;

namespace Assets.Game.View
{
    public static class Shaker
    {
        public static IEnumerator Shake(Transform entityView, float time, float intensity)
        {
            Debug.Log(entityView.localPosition);
            //OBJECT MUST REMAIN FIXED THROUGHOUT THE SHAKE
            Vector2 initPos = new Vector2(entityView.localPosition.x, entityView.localPosition.y);

            float t = time;
            while (t >= 0)
            {
                Vector2 entityPos = new Vector2(entityView.localPosition.x, entityView.localPosition.y);

                Vector2 random = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * intensity;

                float maxBound = intensity * t / time;

                random = new Vector2(Mathf.Clamp(entityPos.x + random.x, initPos.x - maxBound, initPos.x + maxBound), Mathf.Clamp(entityPos.y + random.y, initPos.y - maxBound, initPos.y + maxBound)) - entityPos;

                entityView.localPosition += new Vector3(random.x, random.y, 0);

                t -= Time.deltaTime;
                yield return 0;
            }

            entityView.localPosition = new Vector3(initPos.x, initPos.y, 0);
            Debug.Log(entityView.localPosition);
        }
    }
}
