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
