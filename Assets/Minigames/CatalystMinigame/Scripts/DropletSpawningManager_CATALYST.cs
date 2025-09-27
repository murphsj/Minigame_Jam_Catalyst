using System;
using System.Collections.Generic;
using UnityEngine;

public class DropletSpawningManager_CATALYST : MonoBehaviour
{
    public Droplet_CATACLYST droplet;
    public BoxCollider2D dropletSpawnPlatform;
    public float dropletSpawnHeight = 100;
    public float dropletMoveSpeed = 1;
    public float dropletSpawnRate = 1;

    Dictionary<DropletType, Sprite> spriteMap;

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

    void LoadDropletSpriteMap()
    {
        spriteMap = new Dictionary<DropletType, Sprite>();
        foreach (DropletType dropletType in Enum.GetValues(typeof(DropletType)))
        {
            spriteMap.Add(dropletType, Instantiate(Resources.Load<Sprite>(dropletType.getSpriteName())));
        }

        Debug.Log(spriteMap[DropletType.Blue]);
    }

    void Start()
    {
        CalculateSpawnRange();
        LoadDropletSpriteMap();
    }

    void SpawnDroplet()
    {
        Vector2 position = new Vector2(UnityEngine.Random.Range(minSpawnX, maxSpawnX), spawnY);
        DropletType dropletType = (DropletType)UnityEngine.Random.Range(0, 3);

        Droplet_CATACLYST spawnedDroplet = Instantiate(droplet, position, Quaternion.identity);
        spawnedDroplet.GetComponent<SpriteRenderer>().sprite = spriteMap[dropletType];
        spawnedDroplet.Initialize(dropletType, dropletMoveSpeed);
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
