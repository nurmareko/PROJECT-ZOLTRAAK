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
        Instantiate(waves[waveNumber].enemyPrefab, transform.position, transform.rotation);
        waves[waveNumber].spawnedEnemyCount++;
    } 
}
