using System.Collections;
using UnityEngine;

public class EnemySpawner_CATALYST : MonoBehaviour
{
    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;
    
    [Header("Spawning Settings")]
    public float spawnInterval = 3f;
    public bool autoStart = true;
    public int maxEnemies = 4;
    public bool continuousSpawning = false;
    
    [Header("Wave Spawning")]
    public float[] waveSpawnTimes = { 0f, 10f, 20f, 30f }; // Spawn times in seconds (every 10 seconds)
    public float respawnDelay = 20f; // Respawn delay when enemies are killed
    
    int enemiesSpawned = 0;
    int enemiesKilled = 0;
    bool isSpawning = false;
    bool[] waveSpawned = { false, false, false, false };
    float gameStartTime;
    
    void Start()
    {
        gameStartTime = Time.time;
        if (autoStart)
        {
            StartWaveSpawning();
        }
    }
    
    void Update()
    {
        HandleWaveSpawning();
    }
    
    public void StartWaveSpawning()
    {
        // Start the wave-based spawning system
        Debug.Log("Starting wave-based enemy spawning");
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
    
    void HandleWaveSpawning()
    {
        float currentTime = Time.time - gameStartTime;
        
        // Check each wave spawn time
        for (int i = 0; i < waveSpawnTimes.Length; i++)
        {
            if (!waveSpawned[i] && currentTime >= waveSpawnTimes[i])
            {
                Debug.Log($"Spawning wave enemy {i + 1} at {currentTime:F1}s");
                SpawnEnemy();
                waveSpawned[i] = true;
            }
        }
    }
    
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"Enemy killed! Total killed: {enemiesKilled}");
        
        // Start respawn timer
        StartCoroutine(RespawnEnemyAfterDelay());
    }
    
    IEnumerator RespawnEnemyAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        // Check if we can spawn more enemies (max 4 on field)
        Enemy_CATALYST[] existingEnemies = FindObjectsOfType<Enemy_CATALYST>();
        if (existingEnemies.Length < maxEnemies)
        {
            Debug.Log("Respawning enemy after delay");
            SpawnEnemy();
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
