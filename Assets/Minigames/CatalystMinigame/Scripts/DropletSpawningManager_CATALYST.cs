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
    List<DropletType_CATALYST> spawnBag;

    void PopulateSpawnBag()
    {
        for (int i = 1; i < 5; i++)
        {
            spawnBag.Add((DropletType_CATALYST)i);
        }
    }

    void CalculateSpawnRange()
    {
        minSpawnX = dropletSpawnPlatform.bounds.min.x;
        maxSpawnX = dropletSpawnPlatform.bounds.max.x;
        spawnY = dropletSpawnPlatform.bounds.min.y;
    }

    void Start()
    {
        spawnBag = new List<DropletType_CATALYST>(4);
        CalculateSpawnRange();
        PopulateSpawnBag();
    }

    void SpawnDroplet()
    {
        Vector2 position = new Vector2(UnityEngine.Random.Range(minSpawnX, maxSpawnX), spawnY);
        int index = Random.Range(0, spawnBag.Count-1);
        DropletType_CATALYST dropletType = spawnBag[index];
        spawnBag.RemoveAt(index);
        if (spawnBag.Count == 0) PopulateSpawnBag();

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
