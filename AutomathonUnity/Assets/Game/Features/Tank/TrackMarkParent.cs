using Automathon.Game;
using UnityEngine;

public class TrackMarkParent : MonoBehaviour
{
    [SerializeField] private FadeOutAndTint markPrefab;

    private void Awake()
    {
        TrackMarks.SpawnTrackMark += SpawnTrackMark;
    }

    private void OnDestroy()
    {
        TrackMarks.SpawnTrackMark -= SpawnTrackMark;
    }

    void SpawnTrackMark(Vector2 position, Quaternion rotation, float lifetime, Color color)
    {
        FadeOutAndTint mark = Instantiate(markPrefab, position, rotation);

        mark.SetColor(color);

        mark.transform.parent = transform; //just to keep the scene view organized

        StartCoroutine(mark.Fade(lifetime));
    }
}
