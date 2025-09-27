using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Droplet_CATACLYST : MonoBehaviour
{
    public DropletType dropletType;
    public SpriteAtlas dropletSpriteSheet;

    BoxCollider2D collider;
    SpriteRenderer renderer;
    float moveSpeed;

    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(DropletType dropletType, float moveSpeed)
    {
        this.dropletType = dropletType;
        this.moveSpeed = moveSpeed;
    }

    void Update()
    {
        transform.Translate(new Vector2(0, -Time.deltaTime));
    }
    
    
}
