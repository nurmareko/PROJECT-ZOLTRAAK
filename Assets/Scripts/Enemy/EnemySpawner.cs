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


    // Update is called once per frame
    void Update()
    {
        waves[waveNumber].spawnTimer += Time.deltaTime;
        if (waves[waveNumber].spawnTimer >= waves[waveNumber].spawnInterval)
        {
            waves[waveNumber].spawnTimer = 0;
            SpawnEnemy();
        }
        if (waves[waveNumber].spawnedEnemyCount >= waves[waveNumber].enemiesPerWave)
        {
            waves[waveNumber].spawnedEnemyCount = 0;

            if (waves[waveNumber].spawnInterval > 0.3f)
            {
                waves[waveNumber].spawnInterval *= 0.9f;
            }

            waveNumber++;
        }
        if (waveNumber >= waves.Count)
        {
            waveNumber = 0;
        }
    }

    private void SpawnEnemy()
    {
        Instantiate(waves[waveNumber].enemyPrefab, RandomSpawnPoint(), transform.rotation);
        waves[waveNumber].spawnedEnemyCount++;
    }

    private Vector2 RandomSpawnPoint()
    {
        Vector2 spawnPoint;

        if (Random.Range(0f, 1f) > 0.5)
        {
            spawnPoint.x = Random.Range(minPosn.position.x, maxPosn.position.x);

            if (Random.Range(0f, 1f) > 0.5)
            {
                spawnPoint.y = minPosn.position.y;
            } else
            {
                spawnPoint.y = maxPosn.position.y;
            }

        } else
        {
            spawnPoint.x = Random.Range(minPosn.position.y, maxPosn.position.y);

            if (Random.Range(0f, 1f) > 0.5)
            {
                spawnPoint.y = minPosn.position.x;
            } else
            {
                spawnPoint.y = maxPosn.position.x;
            }
        }

        return spawnPoint;
    }
}
