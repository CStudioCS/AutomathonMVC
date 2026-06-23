using System;
using UnityEngine;

public class TrackMarks : MonoBehaviour
{
    public static event Action<Vector2, Quaternion, float> SpawnTrackMark;

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
                SpawnMark(lastMarkPosition + offset, transform.rotation);
            }

            traveled = Vector3.Distance(current, lastMarkPosition);
        }
    }

    private void SpawnMark(Vector2 position, Quaternion rotation)
        => SpawnTrackMark(position, rotation, markLifetime);
}