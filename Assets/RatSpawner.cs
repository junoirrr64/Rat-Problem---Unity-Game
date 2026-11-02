using System.Collections;
using UnityEngine;

public class RatSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ratPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Timing")]
    [SerializeField] private float minimumInterval = 2f;
    [SerializeField] private float maximumInterval = 5f;

    [Header("Rat Limit")]
    [SerializeField] private int maximumRats = 5;

    // Coroutine that continuously spawns rats at random intervals, ensuring the number of rats does not exceed the maximum limit.
    private IEnumerator Start()
    {
        while (true)
        {
            float waitTime =
                Random.Range(
                    minimumInterval,
                    maximumInterval
                );

            yield return new WaitForSeconds(waitTime);

            SpawnRat();
        }
    }

    // Method to spawn a rat at a random spawn point, ensuring the total number of rats does not exceed the maximum limit.
    private void SpawnRat()
    {
        if (ratPrefab == null ||
            player == null ||
            spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            return;
        }

        int currentRatCount =
            FindObjectsByType<MouseFollow>(
                FindObjectsSortMode.None
            ).Length;

        if (currentRatCount >= maximumRats)
            return;

        Transform spawnPoint =
            spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

        GameObject newRat =
            Instantiate(
                ratPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        MouseFollow ratFollow =
            newRat.GetComponent<MouseFollow>();

        if (ratFollow != null)
        {
            ratFollow.SetTarget(player);
        }
    }
}