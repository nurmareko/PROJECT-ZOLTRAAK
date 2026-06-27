using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;
        public float spawnInterval;
        public int enemiesPerWave;
        public int spawnedEnemyCount;
        public float spawnTimer;
    }

    public List <Wave> waves;
    public int waveNumber;
    public Transform minPosn;
    public Transform maxPosn;
    [SerializeField] private float minimumSpawnInterval = 0.75f;
    [SerializeField] private float spawnIntervalMultiplier = 0.95f;

    void Awake()
    {
        ResetWaveState();
    }

    // Update is called once per frame
    void Update()
    {
        if (waves == null || waves.Count == 0)
        {
            return;
        }

        if (waveNumber < 0 || waveNumber >= waves.Count)
        {
            waveNumber = 0;
        }

        Wave currentWave = waves[waveNumber];

        if (currentWave == null ||
            currentWave.enemyPrefab == null ||
            currentWave.enemiesPerWave <= 0 ||
            currentWave.spawnInterval <= 0f)
        {
            AdvanceWave();
            return;
        }

        currentWave.spawnTimer += Time.deltaTime;
        if (currentWave.spawnTimer >= currentWave.spawnInterval)
        {
            currentWave.spawnTimer = 0;
            SpawnEnemy(currentWave);
        }

        if (currentWave.spawnedEnemyCount >= currentWave.enemiesPerWave)
        {
            currentWave.spawnedEnemyCount = 0;

            if (currentWave.spawnInterval > minimumSpawnInterval)
            {
                currentWave.spawnInterval = Mathf.Max(
                    minimumSpawnInterval,
                    currentWave.spawnInterval * spawnIntervalMultiplier);
            }

            AdvanceWave();
        }
    }

    private void SpawnEnemy(Wave wave)
    {
        Instantiate(wave.enemyPrefab, RandomSpawnPoint(), transform.rotation);
        wave.spawnedEnemyCount++;
    }

    private void AdvanceWave()
    {
        waveNumber++;
        if (waveNumber >= waves.Count)
        {
            waveNumber = 0;
        }
    }

    private Vector2 RandomSpawnPoint()
    {
        if (minPosn == null || maxPosn == null)
        {
            return transform.position;
        }

        Vector2 spawnPoint;
        float minX = Mathf.Min(minPosn.position.x, maxPosn.position.x);
        float maxX = Mathf.Max(minPosn.position.x, maxPosn.position.x);
        float minY = Mathf.Min(minPosn.position.y, maxPosn.position.y);
        float maxY = Mathf.Max(minPosn.position.y, maxPosn.position.y);

        if (Random.Range(0f, 1f) > 0.5)
        {
            spawnPoint.x = Random.Range(minX, maxX);

            if (Random.Range(0f, 1f) > 0.5)
            {
                spawnPoint.y = minY;
            } else
            {
                spawnPoint.y = maxY;
            }

        } else
        {
            spawnPoint.y = Random.Range(minY, maxY);

            if (Random.Range(0f, 1f) > 0.5)
            {
                spawnPoint.x = minX;
            } else
            {
                spawnPoint.x = maxX;
            }
        }

        return spawnPoint;
    }

    private void ResetWaveState()
    {
        if (waves == null)
        {
            return;
        }

        foreach (Wave wave in waves)
        {
            if (wave == null)
            {
                continue;
            }

            wave.spawnedEnemyCount = 0;
            wave.spawnTimer = 0f;
        }
    }
}
