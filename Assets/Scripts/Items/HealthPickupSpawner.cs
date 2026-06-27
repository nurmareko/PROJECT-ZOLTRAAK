using System.Collections.Generic;
using UnityEngine;

public class HealthPickupSpawner : MonoBehaviour
{
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private Transform minPosn;
    [SerializeField] private Transform maxPosn;
    [SerializeField] private float minSpawnInterval = 8f;
    [SerializeField] private float maxSpawnInterval = 14f;
    [SerializeField] private int maxActivePickups = 3;
    [SerializeField] private float minDistanceFromPlayer = 2f;

    private readonly List<HealthPickup> activePickups = new List<HealthPickup>();
    private float spawnTimer;

    private void Start()
    {
        ResetSpawnTimer();
    }

    private void Update()
    {
        if (PlayerController.Instance == null || !PlayerController.Instance.gameObject.activeSelf)
        {
            return;
        }

        RemoveCollectedPickups();

        if (activePickups.Count >= maxActivePickups)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f)
        {
            return;
        }

        SpawnHealthPickup();
        ResetSpawnTimer();
    }

    private void SpawnHealthPickup()
    {
        HealthPickup pickup;
        Vector3 spawnPosition = RandomSpawnPoint();

        if (healthPickupPrefab != null)
        {
            GameObject pickupObject = Instantiate(healthPickupPrefab, spawnPosition, Quaternion.identity);
            pickup = pickupObject.GetComponent<HealthPickup>();

            if (pickup == null)
            {
                pickup = pickupObject.AddComponent<HealthPickup>();
            }
        }
        else
        {
            pickup = HealthPickup.Create(spawnPosition);
        }

        activePickups.Add(pickup);
    }

    private Vector3 RandomSpawnPoint()
    {
        if (minPosn == null || maxPosn == null)
        {
            return transform.position;
        }

        Vector3 spawnPoint = transform.position;
        Vector2 min = minPosn.position;
        Vector2 max = maxPosn.position;

        for (int i = 0; i < 10; i++)
        {
            spawnPoint = new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                0f);

            if (PlayerController.Instance == null ||
                Vector2.Distance(spawnPoint, PlayerController.Instance.transform.position) >= minDistanceFromPlayer)
            {
                return spawnPoint;
            }
        }

        return spawnPoint;
    }

    private void ResetSpawnTimer()
    {
        float minInterval = Mathf.Min(minSpawnInterval, maxSpawnInterval);
        float maxInterval = Mathf.Max(minSpawnInterval, maxSpawnInterval);
        spawnTimer = Random.Range(minInterval, maxInterval);
    }

    private void RemoveCollectedPickups()
    {
        for (int i = activePickups.Count - 1; i >= 0; i--)
        {
            if (activePickups[i] == null)
            {
                activePickups.RemoveAt(i);
            }
        }
    }
}
