using System.Collections;
using UnityEngine;

public class EnemySpawner_CATALYST : MonoBehaviour
{
    [Header("Enemy Prefab")]
    public Enemy_CATALYST enemyPrefab;

    [Header("Spawning Settings")]
    public float spawnIntervalMax = 4.5f;
    public float spawnIntervalMin = 3f;
    public bool autoStart = true;
    public int maxEnemies = 2;
    public bool continuousSpawning = false;
    
    int enemiesSpawned = 0;
    bool isSpawning = false;
    
    void Start()
    {
        if (autoStart)
        {
            StartSpawning();
        }
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            enemiesSpawned = 0;
            StartCoroutine(SpawnEnemies());
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    IEnumerator SpawnEnemies()
    {
        while (isSpawning && (continuousSpawning || enemiesSpawned < maxEnemies))
        {
            SpawnEnemy();
            enemiesSpawned++;
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
        }
        isSpawning = false;
    }
    
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: enemyPrefab is not assigned!");
            return;
        }
        
        bool moveRight = Random.value > 0.5f;
        
        Vector3 spawnPosition = new Vector3(moveRight ? -12f : 12f, -1.8f, 0f);

        // Spawn enemy from prefab
        Enemy_CATALYST enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.Initialize(moveRight);
        enemy.name = "Enemy_" + enemiesSpawned;
        
        BoxCollider2D bc = enemy.GetComponent<BoxCollider2D>();
        if (bc != null)
        {
            bc.isTrigger = true;
        }
    }
    
    // each enemy is named Enemy_0, Enemy_1, etc.
    [ContextMenu("Spawn Single Enemy")]
    public void SpawnSingleEnemy()
    {
        SpawnEnemy();
        enemiesSpawned++; 
    }
    
}
