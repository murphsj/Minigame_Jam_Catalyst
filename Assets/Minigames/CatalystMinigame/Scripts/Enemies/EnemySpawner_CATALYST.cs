using System.Collections;
using UnityEngine;

public class EnemySpawner_CATALYST : MonoBehaviour
{
    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;
    
    [Header("Spawning Settings")]
    public float spawnInterval = 3f;
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
            yield return new WaitForSeconds(spawnInterval);
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

        Vector3 spawnPosition = new Vector3(0f, -1.5f, 0f);
        
        // Spawn enemy from prefab
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.name = "Enemy_" + enemiesSpawned;
        enemy.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
        
        BoxCollider2D bc = enemy.GetComponent<BoxCollider2D>();
        if (bc != null)
        {
            bc.isTrigger = true;
            bc.size = new Vector2(2f, 2f); // Make collider bigger for collision detection
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
