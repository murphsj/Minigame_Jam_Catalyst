using System;
using System.Collections.Generic;
using UnityEngine;

public class DropletSpawningManager_CATALYST : MonoBehaviour
{
    public Droplet_CATACLYST droplet;
    public BoxCollider2D dropletSpawnPlatform;
    public float dropletMoveSpeed = 1;
    public float dropletSpawnRate = 1;
    public float dropletLifetime = 3;
    
    [Header("Powerup Spawning")]
    public GameObject powerupPrefab;
    public float powerupSpawnChance = 0.1f; // 10% chance
    
    private bool powerupExists = false; 

    float minSpawnX;
    float maxSpawnX;
    float spawnY;
    float spawnTimer = 0;

    void CalculateSpawnRange()
    {
        minSpawnX = dropletSpawnPlatform.bounds.min.x;
        maxSpawnX = dropletSpawnPlatform.bounds.max.x;
        spawnY = dropletSpawnPlatform.bounds.min.y;
    }

    void Start()
    {
        CalculateSpawnRange();
    }

    void SpawnDroplet()
    {
        Vector2 position = new Vector2(UnityEngine.Random.Range(minSpawnX, maxSpawnX), spawnY);
        DropletType dropletType = (DropletType)UnityEngine.Random.Range(1, 5);

        Droplet_CATACLYST spawnedDroplet = Instantiate(droplet, position, Quaternion.identity);
        spawnedDroplet.Initialize(dropletType, dropletMoveSpeed, dropletLifetime);
        
        TrySpawnPowerup(position);
    }
    
    void TrySpawnPowerup(Vector2 dropletPosition)
    {
        // Only spawn if no powerup exists and random chance succeeds (if random number =0.05 -> 0.05 < 0.1 then powerup!)
        if (powerupPrefab != null && !powerupExists && UnityEngine.Random.Range(0f, 1f) < powerupSpawnChance)
        {
            Vector2 powerupPosition = new Vector2(dropletPosition.x, UnityEngine.Random.Range(0f, 4f)); // Spawn between 0f and 4f y-axis
            GameObject powerup = Instantiate(powerupPrefab, powerupPosition, Quaternion.identity);
            
            // Add a component to track when this powerup is destroyed
            PowerupTracker tracker = powerup.AddComponent<PowerupTracker>();
            tracker.Initialize(this);
            
            powerupExists = true;
        }
    }
    
    public void OnPowerupDestroyed()
    {
        powerupExists = false;
    }
    
    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            spawnTimer = dropletSpawnRate;
            SpawnDroplet();
        }
    }
}

// Helper class to track when powerup is destroyed
public class PowerupTracker : MonoBehaviour
{
    private DropletSpawningManager_CATALYST spawner;
    
    public void Initialize(DropletSpawningManager_CATALYST manager)
    {
        spawner = manager;
    }
    
    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnPowerupDestroyed();
        }
    }
}
