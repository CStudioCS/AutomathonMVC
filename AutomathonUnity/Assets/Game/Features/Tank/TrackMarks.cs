using UnityEngine;

public class TrackMarks : MonoBehaviour
{
    [SerializeField] private FadeOut markPrefab;
    [SerializeField] private Transform[] trackPoints;
    [SerializeField] private float distanceBetweenMarks = 0.5f;
    [SerializeField] private float markLifetime = 5f;

    private Vector3 lastMarkPosition;

    void Start()
    {
        lastMarkPosition = transform.position;
    }

    void Update()
    {
        Vector3 current = transform.position;
        float traveled = Vector3.Distance(current, lastMarkPosition);

        while (traveled >= distanceBetweenMarks)
        {
            lastMarkPosition = Vector3.MoveTowards(
                lastMarkPosition, current, distanceBetweenMarks);

            foreach (Transform point in trackPoints)
            {
                Vector3 offset = point.position - current;
                FadeOut mark = Instantiate(
                    markPrefab,
                    lastMarkPosition + offset,
                    transform.rotation);
                Destroy(mark, markLifetime);
                StartCoroutine(mark.Fade(markLifetime));

            }

            traveled = Vector3.Distance(current, lastMarkPosition);
        }
    }
}