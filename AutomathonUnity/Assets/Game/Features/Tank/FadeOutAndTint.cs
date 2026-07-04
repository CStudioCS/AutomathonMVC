using System.Collections;
using UnityEngine;

public class FadeOutAndTint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public IEnumerator Fade(float fadeOutTime)
    {
        float alpha = 1f;
        float time = 0;
        Color color = spriteRenderer.color;

        while (alpha > 0)
        {
            time += Time.deltaTime;
            alpha = 1 - (time / fadeOutTime);
            color.a = alpha;
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 0f;
        spriteRenderer.color = color;
        Destroy(gameObject);
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }
}
