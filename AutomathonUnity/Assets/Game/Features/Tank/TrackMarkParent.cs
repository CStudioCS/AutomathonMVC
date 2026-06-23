using UnityEngine;

public class TrackMarkParent : MonoBehaviour
{
    [SerializeField] private FadeOut markPrefab;

    private void Awake()
    {
        TrackMarks.SpawnTrackMark += SpawnTrackMark;
    }

    private void OnDestroy()
    {
        TrackMarks.SpawnTrackMark -= SpawnTrackMark;
    }

    void SpawnTrackMark(Vector2 position, Quaternion rotation, float lifetime)
    {
        FadeOut mark = Instantiate(markPrefab, position, rotation);

        mark.transform.parent = transform; //just to keep the scene view organized

        StartCoroutine(mark.Fade(lifetime));
    }
}
