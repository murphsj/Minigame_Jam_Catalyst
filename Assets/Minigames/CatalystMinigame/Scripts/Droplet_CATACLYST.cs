using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Droplet_CATACLYST : MonoBehaviour
{
    public DropletType dropletType;
    public SpriteAtlas dropletSpriteSheet;

    new BoxCollider2D collider;
    new SpriteRenderer renderer;
    Animator animator;
    float moveSpeed;
    float destroyAtTime;

    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        animator.Play(dropletType.getStateName());
    }

    public void Initialize(DropletType dropletType, float moveSpeed, float lifetime)
    {
        this.dropletType = dropletType;
        this.moveSpeed = moveSpeed;
        destroyAtTime = Time.time + lifetime;
    }

    void Update()
    {
        if (Time.time > destroyAtTime)
        {
            Destroy(gameObject);
        }
        transform.Translate(new Vector2(0, -Time.deltaTime * moveSpeed));
    }
    
    
}
